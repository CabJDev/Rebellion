using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    private string url = "http://192.168.1.71:5098";

    public static LobbyManager Instance;

    private readonly WebServerConnection connection = new();
    public UnityEvent<Message> NewMessageReceived { get; } = new();

    [SerializeField]
    private PlayerManager playerManager;

    private int rebels;
    private int neutrals;

    [SerializeField]
    private TMP_Text lobbyText;
    [SerializeField]
    private GameObject mainMenu;
    [SerializeField]
    private GameObject lobbyCreation;
    [SerializeField]
    private TMP_Text[] playerNames;

    [SerializeField]
    private GameObject playerBackground;

    public string lobbyCode = "";

    bool waitingForCode = false;
    float connectionTimeOut = 30f;

    bool waitingForPlayers = false;

    /*
     * Alignment Lists
     * 0 = null
     * 1 = Loyalists
     * 2 = Rebels
     * 3 = Neutrals
     */

    /*
     * Role Lists
     * 0 = null
     * 1 = Offensive
     * 2 = Defensive
     * 3 = Supportive
     */

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

        await InitAsync();

        NewMessageReceived.AddListener(ServerMessage);
    }

    private void Update()
    {
        if (waitingForCode && lobbyCode == "" && connectionTimeOut > 0)
            connectionTimeOut -= Time.deltaTime;
        else if (waitingForCode && lobbyCode != "")
        {
            waitingForCode = false;
            SetUpLobby();
        }

        for (int i = 0; i < 15; i++)
        {
            if (playerManager.players[i].id != "") playerNames[i].text = playerManager.players[i].name;
            else playerNames[i].text = "";
        }
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
    }

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

    private void Receive(Message msg)
    {
        NewMessageReceived.Invoke(msg);
    }

    public async void CreateLobbyButtonPressed()
    {

        await CreateLobby();

        waitingForCode = true;

        // Alignment selection
        //neutrals = UnityEngine.Random.Range(0, 3);

        //playerManager.PrintPlayers();
        //SceneManager.LoadScene("Gameplay");
    }

    public void StartGame()
    {
        int playerAmount = playerManager.currentPlayerCount;

        if (playerAmount < 3)
        {
            Debug.LogError("Not enough players!");
            return;
        }
        else if (playerAmount == 3 || playerAmount == 4) // Best Case: 3v1 | Worst case: 2v1
        {
            neutrals = 0;
            rebels = 1;
        }
        else if (playerAmount >= 5 && playerAmount < 8) // Best Case: 6 v 2 | Worst case: 3 v 2
        {
            neutrals = 0;
            rebels = 2;
        }
        else if (playerAmount >= 8 && playerAmount < 12) // Best Case: 8 v 3 | Worst case: 3 v 5
        {
            neutrals = Random.Range(1, 2);
            rebels = Random.Range(2, 4);
        }
        else if (playerAmount >= 12) // Best Case: 11 v 4 | Worst Case: 9 v 6
        {
            neutrals = Random.Range(1, 3);
            rebels = Random.Range(3, 5);
        }

        SetNeutrals();
        SetRebels();
        SetLoyalists();

        playerManager.PrintPlayers();
        SceneManager.LoadScene("Gameplay");
    }

    private void SetNeutrals()
    {
        Player[] selectedPlayer = new Player[neutrals];
        for (int i = 0; i < neutrals; ++i)
        {
            Player player = playerManager.players[Random.Range(0, 15)];
            while (selectedPlayer.Contains(player) || player.id == "" || playerManager.IsAligned(player.id))
                player = playerManager.players[Random.Range(0, 15)];

            selectedPlayer[i] = player;

            playerManager.SetAlignment(player.id, 3);
            playerManager.SetRole(player.id, Random.Range(1, 3));
        }
    }

    private void SetRebels()
    {
        Player[] selectedPlayer = new Player[rebels];
        for (int i = 0; i < rebels; ++i)
        {
            Player player = playerManager.players[Random.Range(0, 15)];
            while (selectedPlayer.Contains(player) || player.id == "" || playerManager.IsAligned(player.id))
                player = playerManager.players[Random.Range(0, 15)];

            selectedPlayer[i] = player;

            playerManager.SetAlignment(player.id, 2);
            playerManager.SetRole(player.id, Random.Range(1, 4));
        }
    }

    private void SetLoyalists()
    {
        for (int i = 0; i < playerManager.players.Length; ++i)
        {
            Player player = playerManager.players[i];
            if (player.id != "" && !playerManager.IsAligned(player.id))
            {
                playerManager.SetAlignment(player.id, 1);
                playerManager.SetRole(player.id, Random.Range(1, 4));
            }
        }
    }

    private void SetUpLobby()
    {
        waitingForPlayers = true;
        lobbyText.text = lobbyCode;

        lobbyCreation.SetActive(true);
        mainMenu.SetActive(false);
    }
}
