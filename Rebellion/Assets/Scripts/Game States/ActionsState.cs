using UnityEngine;

public class ActionsState : MonoBehaviour, IGameState
{
    public float GetStateLength() { return 30; }

    public void GameStateActions()
    {
        Debug.Log("Player actions");
    }

    public void Transition()
    {
        Debug.Log("Transition into action state");
        GameStateActions();
    }
}
