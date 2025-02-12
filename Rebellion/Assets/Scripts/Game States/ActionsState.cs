using UnityEngine;

public class ActionsState : MonoBehaviour, IGameState
{
    [SerializeField]
    ServerManager serverManager;

    public float GetStateLength() { return 30; }
    public string GetStateName() { return "Actions"; }

    public void GameStateActions()
    {
        Debug.Log("Player actions");
    }

    public void EndState()
    {
        serverManager.SendData("Action state ended!");
    }

    public void Transition()
    {
        Debug.Log("Transition into action state");
        Debug.Log(serverManager.RetrieveData(2));
        GameStateActions();
    }
}
