using UnityEngine;
using UnityEngine.SceneManagement;

public class TestScript : MonoBehaviour
{
    [SerializeField]
    private PlayerManager playerManager;
    private void Awake()
    {
        playerManager = PlayerManager.Instance;

        if (playerManager == null)
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
