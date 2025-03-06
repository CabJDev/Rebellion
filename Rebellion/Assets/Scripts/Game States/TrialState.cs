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

    private Dictionary<string, int> playerVotes = new Dictionary<string, int>();

    private void Start()
    {
        playerManager = PlayerManager.Instance;
        serverManager = ServerManager.Instance;

        foreach (Player player in playerManager.players)
            if (player.id != "") playerVotes.Add(player.id, 0);
    }

    public void UpdateVotes(Dictionary<int, int> votes)
    {
        foreach (int playerID in votes.Keys)
        {
            if (votes[playerID] != 0) Debug.Log(playerID + " " + votes[playerID]);
            if (votes[playerID] >= Mathf.RoundToInt(playerManager.playerCount / 2) + 1)
            {
                Debug.Log("Player " + playerID + " has been voted to trial!");
            }
        }
    }

    public void EndState()
    {
    }

    public void Transition()
    {
        Debug.Log("Transition into trial state");
        GameStateActions();
    }
}
