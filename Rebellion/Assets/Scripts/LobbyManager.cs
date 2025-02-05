using Unity.VisualScripting;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField]
    private PlayerManager playerManager;

    // Temporary - For testing only
    [SerializeField]
    private int testPlayers;
    [SerializeField]
    private int rebels;
    [SerializeField]
    private int neutrals;

    public void ButtonPressed()
    {
        for (int i = 0; i < testPlayers; ++i)
        {
            playerManager.AddPlayer("Player_" + i);
        }
    }
}
