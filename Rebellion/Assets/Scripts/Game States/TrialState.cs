using UnityEngine;

public class TrialState : MonoBehaviour, IGameState
{
    public float GetStateLength() { return 30; }

    public void GameStateActions()
    {
        Debug.Log("Player trials phase");
    }

    public void Transition()
    {
        Debug.Log("Transition into trial state");
        GameStateActions();
    }
}
