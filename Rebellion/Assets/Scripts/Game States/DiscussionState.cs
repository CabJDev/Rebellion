using UnityEngine;

public class DiscussionState : MonoBehaviour, IGameState
{
    [SerializeField]
    ServerManager serverManager;

    public float GetStateLength() { return 30; }

    public void GameStateActions()
    {
        Debug.Log("Player dicussion phase");
    }

    public void EndState()
    {
        serverManager.SendData("Discussion state ended!");
    }

    public void Transition()
    {
        Debug.Log("Transition into discussion state");
        Debug.Log(serverManager.RetrieveData());
        GameStateActions();
    }
}
