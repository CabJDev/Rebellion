using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ServerManager : MonoBehaviour
{
	//private string url = "https://192.168.1.65:443";
	private string url = "https://rebelliongame.fun";

	public static ServerManager Instance;

	private PlayerManager playerManager;
	private RoleManager roleManager;

	private readonly WebServerConnection connection = new();
	public UnityEvent<Message> NewMessageReceived { get; } = new();

	public string lobbyCode = "";

	public bool waitingForPlayers = false;

	private Role[][] roles;

	public Dictionary<string, string> targets = new Dictionary<string, string>();
	public Dictionary<string, int> remainingSelfTargets = new Dictionary<string, int>();

	public List<Player> recentlyDead = new List<Player>();

	public Dictionary<Player, int> votes = new Dictionary<Player, int>();
	public Dictionary<Player, Player> lastVoted = new Dictionary<Player, Player>();

	public int currentState;

	public bool endDayEarly = false;

	public bool gameFinished = false;

	public List<string> winners = new List<string>();

	private List<long> timeMetrics = new List<long>();

	public bool gameStarted = false;

	public string winnersText = "";

	public bool pauseGame;
	public float pauseGameTimer;

	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	private async void Start()
	{
		playerManager = PlayerManager.Instance;
		roleManager = RoleManager.Instance;

		await InitAsync();

		NewMessageReceived.AddListener(ServerMessage);
	}

	private async void OnApplicationQuit()
	{
		await connection.CloseConnectionAsync();
	}

	public async void ServerMessage(Message message)
	{
		// Lobby creation/user connects/user disconnects
		if (message.Type == "CreateRoom") lobbyCode = message.Content;
		else if (message.Type == "AddPlayer")
		{
			string[] content = message.Content.Split(',');

			if (!waitingForPlayers)
			{
				await connection.AddPlayerAsync(content[0], lobbyCode, content[1], false);
			}

			bool success = playerManager.AddPlayer(content[2], content[1]);

			await connection.AddPlayerAsync(content[0], lobbyCode, content[1], success);
		}
		else if (message.Type == "RemovePlayer") { playerManager.RemovePlayer(message.Content); }
		else if (message.Type == "RetrieveRoleInfo") 
		{
			if (message.Content == null) return;
			string[] content = message.Content.Split(',');

			timeMetrics.Add(CalculateMS(long.Parse(content[1])));

			Player player = playerManager.players[playerManager.GetPlayerIndex(content[0])];
			await connection.SendRoleInfoAsync(player.id, roleManager.GetRole(player.alignment, player.role));
		}
		else if (message.Type == "RetrievePlayerNames")
		{
			if (message.Content == null) return;
			string[] content = message.Content.Split(',');

			timeMetrics.Add(CalculateMS(long.Parse(content[1])));

			Player player = playerManager.GetPlayer(content[0]);

			Player[] players = playerManager.players;
			string[] names = new string[15];
			int[] specials = new int[15];
			for (int i = 0; i < 15; ++i)
			{
				names[i] = players[i].name;
				if (players[i].id != player.id && player.alignment == 2 && players[i].alignment == 2)
					specials[i] = 2;
				else if (players[i].id == player.id)
					specials[i] = 1;
				else
					specials[i] = 0;
			}
				
			await connection.SendNamesAsync(content[0], names, specials);
		} // Lobby creation/user connects/user disconnects
		else if (message.Type == "PlayerSendMessage") // Player chatting
		{
			if (message.Content == null) return;
			int splitMark = message.Content.IndexOf(',');
			string hash = message.Content.Substring(0, splitMark);
			string playerMessage = message.Content.Substring(splitMark + 1);

			Player sender = playerManager.players[playerManager.GetPlayerIndex(hash)];

			List<string> hashesToSend = new List<string>();

			if (sender.alive)
			{
				if (currentState == 2)
				{
					if (sender.alignment == 2)
					{
						foreach (Player otherPlayer in playerManager.players)
							if (otherPlayer.alignment == 2 && otherPlayer.id != sender.id) hashesToSend.Add(otherPlayer.id);
					}
					else return;
				}
				else
				{
					foreach (Player otherPlayer in playerManager.players)
						if (otherPlayer.id != "" && otherPlayer.id != sender.id) hashesToSend.Add(otherPlayer.id);
				}
			}
			else
				foreach (Player otherPlayer in playerManager.players)
					if (otherPlayer.id != "" && otherPlayer.id != sender.id && !otherPlayer.alive) hashesToSend.Add(otherPlayer.id);

			await connection.PlayerMessageAsync(sender.name, hashesToSend.ToArray(), playerMessage);
		}
		else if (message.Type == "SetPlayerTarget") // Player actions/voting
		{
			if (message.Content == null) return;
			string[] messageContent = message.Content.Split(',');

			if (messageContent.Length != 3) return;

			timeMetrics.Add(CalculateMS(long.Parse(messageContent[2])));

			int playerIndex = playerManager.GetPlayerIndex(messageContent[0]);
			if (playerIndex == -1) return;
			Player player = playerManager.players[playerIndex];
			if (!player.alive) return;

			int targetIndex = int.Parse(messageContent[1]) - 1;

			if (currentState == 2)
			{
				if (targetIndex < 0 || targetIndex >= 15)
				{
					if (targets.ContainsKey(player.id))
						targets[player.id] = "";
					return;
				}

				Player target = playerManager.players[targetIndex];
				if (!target.alive) return;

				if (!ValidateTarget(player, target)) return;

				if (targets.ContainsKey(player.id))
					targets[player.id] = target.id;
				else
					targets.Add(player.id, target.id);
			}
			else if (currentState == 1)
			{
				if (targetIndex < 0 || targetIndex >= 15)
				{
					if (lastVoted.ContainsKey(player))
					{
						votes[lastVoted[player]]--;
						Task systemMsg1 = SystemMessage($"{player.name} has revoked their vote for {lastVoted[player].name}!");
						lastVoted.Remove(player);
					}

					return;
				}
				Player target = playerManager.players[targetIndex];
				if (!target.alive) return;

				if (!target.alive || player.id == target.id) return;

				if (votes.ContainsKey(target))
					votes[target]++;
				else
					votes.Add(target, 1);

				Task systemMsg = SystemMessage($"{player.name} voted for {target.name}!");
				lastVoted.Add(player, target);
			}
		}
	}

	private bool ValidateTarget(Player player, Player target)
	{
		Role playerRole = roleManager.GetRole(player.alignment, player.role);

		string[] targets = playerRole.targets.Split(',');
		if (targets[0] == "Self")
		{
			if (remainingSelfTargets.ContainsKey(player.id) && remainingSelfTargets[player.id] >= int.Parse(targets[1])) return false;
			if (player.id != target.id) return false;
		}
		else if (targets[0] == "All")
		{
			if (player.id == target.id) return false;
		}
		else if (targets[0] == "Faction")
		{
			if (player.id == target.id) return false;
			if (player.alignment != target.alignment) return false;
		}
		else if (targets[0] == "NonFaction")
		{
			if (player.id == target.id) return false;
			if (player.alignment == target.alignment) return false;
		}

		return true;
	}

	private void Receive(Message msg)
	{
		NewMessageReceived.Invoke(msg);
	}

	// Lobby creation tasks
	private async Task InitAsync()
	{
		await connection.InitAsync<Message>($"{url}/ClientHub", "ReceiveMessage");
		connection.OnMessageReceived += Receive;
	}

	public async Task SendAsync(Message msg)
	{
		await connection.SendMessageAsync(msg);
	}

	public async Task PlayerSystemMessage(string hash, string message)
	{
		await connection.PlayerSystemMessageAsync(hash, message);
	}

	public async Task CreateLobby() { await connection.CreateLobbyAsync(); }

	public async Task StartGame() 
	{
		gameStarted = true;
		await connection.StartGameAsync(lobbyCode); 
	}
	// Lobby creation tasks

	// Action state tasks
	public async Task EnableButtons()
	{
		Player[] players = playerManager.players;

		for (int i = 0; i < 15; i++)
		{
			if (players[i].id == "") continue;
			if (players[i].alive)
			{
				int[] toEnable = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
				/*
				 * Targets
				 * All
				 * NonFaction
				 * Faction
				 * Self
				 *
				 * Modifier
				 * Number (how many times they can do it)
				 */


				Role playerRole = roleManager.GetRole(players[i].alignment, players[i].role);
				string target = playerRole.targets.Split('+')[0];

				if (target == "All")
				{
					for (int j = 0; j < 15; j++)
						if (PlayerAlive(players[j]) && players[j].id != players[i].id) toEnable[j] = 1;
				}
				else if (target == "NonFaction")
				{
					for (int j = 0; j < 15; j++)
						if (PlayerAlive(players[j]) && !playerManager.rebels.Contains(players[j].id) && players[j].id != players[i].id) toEnable[j] = 1;
				}
				else if (target == "Faction")
				{
					for (int j = 0; j < 15; j++)
						if (PlayerAlive(players[j]) && playerManager.rebels.Contains(players[j].id) && players[j].id != players[i].id) toEnable[j] = 1;
				}
				else if (target == "Self")
				{
					if (remainingSelfTargets.ContainsKey(players[i].id) && remainingSelfTargets[players[i].id] > int.Parse(target[1].ToString())) toEnable[i] = 0;
					else if (players[i].alive) toEnable[i] = 1;
				}

				await connection.EnableButtonsAsync(players[i].id, toEnable);
			}
			else await connection.EnableButtonsAsync(players[i].id, new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
		}
	}

	public async Task EnableVoteButtons()
	{
		Player[] players = playerManager.players;

		for (int i = 0; i < 15; ++i)
		{
			if (players[i].id == "" || players[i].alive == false) continue;
			int[] toEnable = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

			for (int j = 0; j < 15; ++j)
			{
				if (players[j].id == players[i].id || !players[j].alive) continue;
				toEnable[j] = 1;
			}

			await connection.EnableButtonsAsync(players[i].id, toEnable);
		}
	}

	public async Task PlayerKilled(Player player)
	{
		recentlyDead.Add(player);
		int playerIndex = playerManager.GetPlayerIndex(player.id);
		playerManager.players[playerIndex].alive = false;

		playerManager.currentPlayerCount--;

		if (player.alignment == 3 && player.role == 2) winners.Remove(player.id);

		string deadPlayerHash = player.id;
		if (playerManager.rebels.Contains(deadPlayerHash))
		{
			playerManager.rebels.Remove(deadPlayerHash);
			if (playerManager.offensiveRebels.Contains(deadPlayerHash))
			{
				playerManager.offensiveRebels.Remove(deadPlayerHash);

				if (playerManager.offensiveRebels.Count == 0 && playerManager.rebels.Count > 0)
				{
					string newAggressiveRebel = playerManager.rebels[UnityEngine.Random.Range(0, playerManager.rebels.Count)];
					playerManager.SetRole(newAggressiveRebel, 1);
					Task playerMsg = PlayerSystemMessage(newAggressiveRebel, "You have become a Rebel Daggerfang!");
				}
			}
		}
        else if (playerManager.loyalists.Contains(deadPlayerHash)) playerManager.loyalists.Remove(deadPlayerHash);
		else if (playerManager.offensiveNeutrals.Contains(deadPlayerHash)) playerManager.offensiveNeutrals.Remove(deadPlayerHash);

        await connection.PlayerKilledAsync(lobbyCode, playerIndex + 1);
	}

	public async Task DisableButtons()
	{
		await connection.DisableButtonsAsync(lobbyCode);
	}

	public async Task SystemMessage(string message)
	{
		await connection.SystemMessageAsync(lobbyCode, message);
	}

	public void CheckWinConditions()
	{
		if (playerManager.rebels.Count == 0 && playerManager.offensiveNeutrals.Count == 0 && playerManager.loyalists.Count > 0)
		{
			gameFinished = true;
			foreach (Player player in playerManager.players)
			{
				if (player.id == "" || player.alignment != 1) continue;
				winners.Add(player.id);
			}

			Task systemMsg = SystemMessage("The rebels have all been executed! The Loyalists win! Long live the king!");
			winnersText = "Loyalists win!";
		}
		else if (playerManager.rebels.Count == 0 && playerManager.offensiveNeutrals.Count > 0 && playerManager.loyalists.Count == 0)
		{
			gameFinished = true;
			foreach (Player player in playerManager.players)
			{
				if (player.id == "" || (player.alignment != 3 && player.role != 1)) continue;
				winners.Add(player.id);
			}

			Task systemMsg = SystemMessage("The kingdom has fallen to ruins... The Madcaps win!");
			winnersText = "Madcaps win!";
		}
		else if (playerManager.rebels.Count >= Mathf.FloorToInt((playerManager.currentPlayerCount + 1) / 2) && playerManager.offensiveNeutrals.Count == 0 && playerManager.loyalists.Count == 0)
		{
			gameFinished = true;
			foreach (Player player in playerManager.players)
			{
				if (player.id == "" || player.alignment != 2) continue;
				winners.Add(player.id);
			}

			Task systemMsg = SystemMessage("The rebels have gained enough power to take over the kingdom! The Rebels win!");
			winnersText = "Rebels win!";
		}
		else if (playerManager.rebels.Count == 0 && playerManager.offensiveNeutrals.Count == 0 && playerManager.loyalists.Count == 0)
		{
			gameFinished = true;
			if (playerManager.currentPlayerCount == 0)
			{
				Task systemMsg = SystemMessage("The kingdom lies in ruins... There is no one left alive...");
				winnersText = "Draw!";
			}
			else
			{
				Task systemMsg = SystemMessage("The kingdom lies in ruins, but there is still some hope...");
				winnersText = "Survivors win!";
			}
		}

		if (gameFinished)
		{
			foreach (Player player in playerManager.players)
			{
				if (player.id == "") continue;
				if (winners.Contains(player.id))
				{
					Task systemMsg = PlayerSystemMessage(player.id, "You won!");
				}
				else
				{
					Task systemMsg = PlayerSystemMessage(player.id, "You lost!");
				}
			}

			GameEnd();
		}
	}

	public List<long> GetTimeMetrics() { return timeMetrics; }

	public async Task EndGameAsync()
	{
		await connection.CloseConnectionAsync();
	}

	private void GameEnd()
	{
		SceneManager.LoadScene(2);
	}

	private bool PlayerAlive(Player player) { return player.id != "" && player.alive; }

	private long CalculateMS(long ms) { return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() - ms; }
}