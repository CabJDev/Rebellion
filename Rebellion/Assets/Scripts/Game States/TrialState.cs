using UnityEngine;

public class TrialState : MonoBehaviour, IGameState
{
    [SerializeField]
    ServerManager serverManager;

    public float GetStateLength() { return 30; }

    public void GameStateActions()
    {
        Debug.Log("Player trials phase");
    }

    public void EndState()
    {
        serverManager.SendData("Trial state ended!");
    }

    public void Transition()
    {
        Debug.Log("Transition into trial state");
        Debug.Log(serverManager.RetrieveData());
        GameStateActions();
    }
}
