using UnityEngine;

public class DiscussionState : MonoBehaviour, IGameState
{
    public float GetStateLength() { return 30; }

    public void GameStateActions()
    {
        Debug.Log("Player dicussion phase");
    }

    public void Transition()
    {
        Debug.Log("Transition into discussion state");
        GameStateActions();
    }
}
