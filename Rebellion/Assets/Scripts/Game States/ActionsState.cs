using System.Threading.Tasks;
using UnityEngine;

public class ActionsState : MonoBehaviour, IGameState
{
    [SerializeField]
    ServerManager serverManager;

    public float GetStateLength() { return 30; }
    public string GetStateName() { return "Actions"; }

    private void Start() { serverManager = ServerManager.Instance; }

	public void GameStateActions()
    {

    }

    public void EndState()
    {
        
    }

    public void Transition()
    {
        Task buttonsEnabled = serverManager.EnableButtons();
		GameStateActions();
    }
}
