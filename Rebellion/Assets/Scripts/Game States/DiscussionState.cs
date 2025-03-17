using System.Threading.Tasks;
using UnityEngine;

public class DiscussionState : MonoBehaviour, IGameState
{
    ServerManager serverManager;
    PlayerManager playerManager;

    [SerializeField] private StateManager stateManager;

    public float GetStateLength() { return 45; }
    public string GetStateName() { return "Discussion"; }

	private void Start()
    {
        playerManager = PlayerManager.Instance;
        serverManager = ServerManager.Instance; 
    }

	public void GameStateActions()
    {
    }

    public void EndState()
    {
	}

	public void Transition()
    {
		
		foreach (Player deadPlayer in serverManager.recentlyDead)
        {
            Task serverMsg = serverManager.SystemMessage($"{deadPlayer.name} died last night.");
            int playerIndex = playerManager.GetPlayerIndex(deadPlayer.id);

            stateManager.ShowDeadPlayer(playerIndex);
        }

        stateManager.ShowAlivePlayer();

		serverManager.recentlyDead.Clear();
		serverManager.CheckWinConditions();

		Task dayMsg = serverManager.SystemMessage($"The king has called for a council meeting!");

		GameStateActions();
    }
}
