using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [SerializeField]
    private PlayerManager playerManager;

    // Temporary - For testing only
    [SerializeField]
    private int testPlayers;
    [SerializeField]
    private int rebels;
    private int neutrals;

    /*
     * Alignment Lists
     * 0 = null
     * 1 = Loyalists
     * 2 = Rebels
     * 3 = Neutrals
     */

    /*
     * Role Lists
     * 0 = null
     * 1 = Offensive
     * 2 = Defensive
     * 3 = Supportive
     */

    private void Start()
    {
        playerManager = PlayerManager.Instance;
    }

    // TODO: Set up actual lobby creation
    public void ButtonPressed()
    {
        // Alignment selection
        neutrals = Random.Range(0, 3);

        for (int i = 0; i < testPlayers; ++i)
        {
            playerManager.RemovePlayer(i);
            playerManager.AddPlayer("Player_" + (i + 1));
        }

        int[] rebelIDs = new int[rebels];

        for (int i = 0; i < rebels; ++i)
        {
            int id = Random.Range(1, testPlayers);
            while (rebelIDs.Contains(id)) id = Random.Range(1, testPlayers);
            rebelIDs[i] = id;
            playerManager.SetAlignment(id, 2);

            if (i == 0)
                playerManager.SetRole(id, 1);
            else
                playerManager.SetRole(id, Random.Range(1, 4));
        }

        for (int i = 0; i < neutrals; ++i)
        {
            int id = Random.Range(1, testPlayers);

            while (rebelIDs.Contains(id))
                id = Random.Range(1, testPlayers);

            playerManager.SetAlignment(id, 3);

            if (i == 0)
                playerManager.SetRole(id, 1);
            else
                playerManager.SetRole(id, Random.Range(1, 4));
        }

        for (int i = 0; i < testPlayers; ++i)
        {
            if (playerManager.IsPlayer(i) && !playerManager.IsAligned(i))
            {
                playerManager.SetAlignment(i, 1);
                playerManager.SetRole(i, Random.Range(1, 4));
            }
        }

        playerManager.PrintPlayers();
        SceneManager.LoadScene("Gameplay");
    }
}
