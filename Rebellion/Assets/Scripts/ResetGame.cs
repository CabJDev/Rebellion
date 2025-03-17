using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetGame : MonoBehaviour
{
	ServerManager serverManager = ServerManager.Instance;
	PlayerManager playerManager = PlayerManager.Instance;
	RoleManager roleManager = RoleManager.Instance;

	[SerializeField] TMP_Text winnerText;
	[SerializeField] Material shader;

	private void Start()
	{
		shader.SetFloat("_DensityMultiplier", 0f);
		winnerText.text = serverManager.winnersText;
		List<long> timeMetrics = serverManager.GetTimeMetrics();
		if (timeMetrics.Count > 0)
		{
			long average = 0;
			long lowest = timeMetrics[0];
			long highest = timeMetrics[0];

			foreach (long metric in timeMetrics)
			{
				average += metric;
				if (metric < lowest) lowest = metric;
				if (metric > highest) highest = metric;
			}

			average /= timeMetrics.Count;

			try
			{
				DateTime time = DateTime.Now;
				StreamWriter sw = new StreamWriter(time.ToString("dd-MM-yyyy HH-mm-ss") + " Metrics.txt");
				sw.WriteLine("Average: " + average);
				sw.WriteLine("Lowest: " + lowest);
				sw.WriteLine("Highest: " + highest);
				sw.Close();
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}
	}

	public void MenuScreen()
	{
		Task endGame = serverManager.EndGameAsync();
		Destroy(serverManager.gameObject);
		Destroy(playerManager.gameObject);
		Destroy(roleManager.gameObject);
		SceneManager.LoadScene(0);
	}
}
