using System.Collections.Generic;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    PlayerManager playerManager;

    [SerializeField]
    TrialState trialState;

    private Dictionary<string, int> playerVotes = new Dictionary<string, int>();

    private bool isVoting = false;

    private float nextCall = 1f; // Ensure that it doesn't spam the web server with requests for votes

    private void Start()
    {
        playerManager = PlayerManager.Instance;

        foreach (Player player in playerManager.players)
        {
            if (player.id != "") playerVotes.Add(player.id, 0);
        }   
    }

    // TEMPORARY
    int playerCount = 15;

    private void Update()
    {
        
    }

    // Temporary solution to get and send data
    // TODO: Connect with web server

    public void SendData(string data)
    {
        Debug.Log(data);
    }

    public string RetrieveData(int state)
    {
        if (state == 1)
        {
            isVoting = true;
            return "Voting session started!";
        }
        else return "Player data retrieved";
    }

    public void StopVoting() { isVoting = false; }
}
