using UnityEngine;

public class DiscussionState : MonoBehaviour, IGameState
{
    [SerializeField]
    ServerManager serverManager;

    public float GetStateLength() { return 5; }
    public string GetStateName() { return "Discussion"; }

	private void Start() { serverManager = ServerManager.Instance; }

	public void GameStateActions()
    {
        Debug.Log("Player dicussion phase");
    }

    public void EndState()
    {
    }

    public void Transition()
    {
        Debug.Log("Transition into discussion state");
        GameStateActions();
    }
}
