using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TrialState : MonoBehaviour, IGameState
{
    ServerManager serverManager;
    PlayerManager playerManager;
    RoleManager roleManager;

    public float GetStateLength() { return 30; }
    public string GetStateName() { return "Trial"; }

    private Player playerToExecute;

    public void GameStateActions()
    {
        
    }

    private bool roleRevealed = false;
    private bool bamboozled = false;

    private void Start()
    {
        playerManager = PlayerManager.Instance;
        serverManager = ServerManager.Instance;
        roleManager = RoleManager.Instance;
    }

	private void Update()
	{
        if (serverManager.currentState != 1) return;
        if (!serverManager.pauseGame)
        {
			int majority = Mathf.CeilToInt((playerManager.currentPlayerCount + 2) / 2);
			foreach (Player player in serverManager.votes.Keys)
			{
				if (serverManager.votes[player] >= majority)
				{
					serverManager.pauseGame = true;
                    serverManager.pauseGameTimer = 5f;
                    playerToExecute = player;
					Task buttonsDisabled = serverManager.DisableButtons();
					Task systemMsg = serverManager.SystemMessage($"The council has deemed {playerToExecute.name} to be a rebel!");
                    Task killPlayer = serverManager.PlayerKilled(player);

					serverManager.votes.Clear();
					break;
				}
			}
		} else
        {
            if (serverManager.currentState == 1 && serverManager.pauseGameTimer > 0f) serverManager.pauseGameTimer -= Time.deltaTime;
			else
            {
                if (!roleRevealed)
                {
                    Role playerRole = roleManager.GetRole(playerToExecute.alignment, playerToExecute.role);
                    Task roleRevealMsg = serverManager.SystemMessage($"{playerToExecute.name}'s role was... {playerRole.roleName}!");
                    if (playerToExecute.alignment == 3 && playerToExecute.role == 3)
                    {
                        Task bamboozledMsg = serverManager.SystemMessage($"{playerToExecute.name} has bamboozled the kingdom!");
                        serverManager.winners.Add(playerToExecute.id);
                        bamboozled = true;
                    }
                    roleRevealed = true;
                    serverManager.pauseGameTimer = 5f;
                }
                else if (roleRevealed)
                {
                    if (bamboozled && playerManager.playerCount > 1)
                    {
						Player player = playerManager.players[Random.Range(0, 15)];

						while (player.id == "" || !player.alive)
							player = playerManager.players[Random.Range(0, 15)];

						Task bamboozledDeathMsg = serverManager.SystemMessage($"{player.name} was so bamboozled that they died!");
						Task killPlayer = serverManager.PlayerKilled(player);
						bamboozled = false;
					}
                    serverManager.endDayEarly = true;
                }
			}
		}
	}

	public void EndState()
    {
        Task systemMsg = serverManager.SystemMessage("The king has decided to end the day's council assembly.");
        roleRevealed = false;
		serverManager.votes.Clear();
	}

    public void Transition()
    {
		Task serverMsg = serverManager.SystemMessage("The king is asking for the council's opinion on who to exile!");
		Task buttonsEnabled = serverManager.EnableVoteButtons();
		GameStateActions();
    }
}
