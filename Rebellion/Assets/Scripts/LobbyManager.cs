using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    private readonly WebServerConnection connection = new();
    public UnityEvent<Message> NewMessageReceived { get; } = new();

    [SerializeField]
    private PlayerManager playerManager;

    // Temporary - For testing only
    [SerializeField]
    private int testPlayers;
    [SerializeField]
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
        for (int i = 0; i < 15; i++)
        {
            if (playerManager.players[i].id != "") playerNames[i].text = playerManager.players[i].name;
            else playerNames[i].text = "";
        }
    }

    private async void OnApplicationQuit()
    {
        Message msg = new Message();
        msg.Type = "CloseConnection";
        msg.Content = "";
        await SendAsync(msg);
    }

    public async void ServerMessage(Message message)
    {
        if (message.Type == "CreateRoom") lobbyCode = message.Content;
        else if (message.Type == "AddPlayer")
        {
            string[] content = message.Content.Split(',');

            bool success = playerManager.AddPlayer(content[0], content[1]);

            await connection.AddPlayerAsync(content[0], lobbyCode, success);
        }
    }

    private async Task InitAsync()
    {
        await connection.InitAsync<Message>("https://localhost:7003/ClientHub", "ReceiveMessage");
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

    // TODO: Set up actual lobby creation
    public async void ButtonPressed()
    {
        await CreateLobby();

        while (lobbyCode == "") ;

        lobbyText.text = lobbyCode;

        lobbyCreation.SetActive(true);
        mainMenu.SetActive(false);

        // Alignment selection
        //neutrals = UnityEngine.Random.Range(0, 3);

        //playerManager.PrintPlayers();
        //SceneManager.LoadScene("Gameplay");
    }
}
