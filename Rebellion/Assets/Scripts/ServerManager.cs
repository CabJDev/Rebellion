using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class ServerManager : MonoBehaviour
{
	private string url = "http://192.168.1.71:5098";
	//private string url = "http://localhost:5098";

	public static ServerManager Instance;

	[SerializeField] PlayerManager playerManager;
	private RoleManager roleManager;

	private readonly WebServerConnection connection = new();
	public UnityEvent<Message> NewMessageReceived { get; } = new();

	public string lobbyCode = "";

	public bool waitingForPlayers = false;

	private Role[][] roles;

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
		Message msg = new()
		{
			Type = "CloseConnection",
			Content = ""
		};

		await SendAsync(msg);
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
			Player player = playerManager.players[playerManager.GetPlayerIndex(message.Content)];
			await connection.SendRoleInfoAsync(player.id, roleManager.GetRole(player.alignment, player.role));
		}
		else if (message.Type == "RetrievePlayerNames")
		{
			if (message.Content == null) return;
			Player[] players = playerManager.players;
			string[] names = new string[15];
			for (int i = 0; i < 15; ++i)
			{
				names[i] = players[i].name;
			}
				
			await connection.SendNamesAsync(message.Content, names);
		} // Lobby creation/user connects/user disconnects
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

	public async Task CreateLobby() { await connection.CreateLobbyAsync(); }

	public async Task StartGame() 
	{
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
						if (PlayerAlive(players[j]) && !playerManager.rebels.Contains(players[j]) && players[j].id != players[i].id) toEnable[j] = 1;
				}
				else if (target == "Faction")
				{
					for (int j = 0; j < 15; j++)
						if (PlayerAlive(players[j]) && playerManager.rebels.Contains(players[j]) && players[j].id != players[i].id) toEnable[j] = 1;
				}
				else if (target == "Self")
				{
					if (players[i].alive) toEnable[i] = 1;
				}

				await connection.EnableButtonsAsync(players[i].id, toEnable);
			}
			else await connection.EnableButtonsAsync(players[i].id, new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
		}
	}

	private bool PlayerAlive(Player player) { return player.id != "" && player.alive; }
}
