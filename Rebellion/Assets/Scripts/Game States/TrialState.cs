using System.Collections.Generic;
using UnityEngine;

public class TrialState : MonoBehaviour, IGameState
{
    [SerializeField]
    ServerManager serverManager;

    PlayerManager playerManager;

    private int currentTrialCount;

    public float GetStateLength() { return 30; }
    public string GetStateName() { return "Trial"; }

    public void GameStateActions()
    {
        // Temporary random player trial voting

    }

    private Dictionary<int, int> playerVotes = new Dictionary<int, int>();

    private void Start()
    {
        playerManager = PlayerManager.Instance;

        foreach (Player player in playerManager.players)
            if (player.id != 0) playerVotes.Add(player.id, 0);
    }

    public void UpdateVotes(Dictionary<int, int> votes)
    {
        foreach (int playerID in votes.Keys)
        {
            if (votes[playerID] != 0) Debug.Log(playerID + " " + votes[playerID]);
            if (votes[playerID] >= Mathf.RoundToInt(playerManager.playerCount / 2) + 1)
            {
                Debug.Log("Player " + playerID + " has been voted to trial!");
                serverManager.StopVoting();
            }
        }
    }

    public void EndState()
    {
        serverManager.SendData("Trial state ended!");
        serverManager.StopVoting();
    }

    public void Transition()
    {
        Debug.Log("Transition into trial state");
        Debug.Log(serverManager.RetrieveData(1));
        GameStateActions();
    }
}
