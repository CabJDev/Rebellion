using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    private PlayerManager playerManager;
    private RoleManager roleManager;
    private ServerManager serverManager;

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

    bool waitingForCode = false;
    float connectionTimeOut = 30f;

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

    private void Start()
    {
        playerManager = PlayerManager.Instance;
        roleManager = RoleManager.Instance;
        serverManager = ServerManager.Instance;
    }

    private void Update()
    {
        if (waitingForCode && serverManager.lobbyCode == "" && connectionTimeOut > 0)
            connectionTimeOut -= Time.deltaTime;
        else if (waitingForCode && serverManager.lobbyCode != "")
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

    public async void CreateLobbyButtonPressed()
    {

        await serverManager.CreateLobby();

        waitingForCode = true;
    }

    public async void StartGameButton()
    {
        int playerAmount = playerManager.currentPlayerCount;

        // TODO: SET MIN PLAYER AMOUNT TO 3!!

        if (playerAmount == 1)
        {
            Player player = playerManager.players[0];
            playerManager.SetAlignment(player.id, 3);
            playerManager.SetRole(player.id, 2);

			playerManager.PrintPlayers();
			await serverManager.StartGame();
			SceneManager.LoadScene("Gameplay");
            return;
		}
        if (playerAmount == 2) // TODO: SET TO DO NOTHING IF LESS THAN 3 PLAYERS!!!
        {

			playerManager.PrintPlayers();
			await serverManager.StartGame();
			SceneManager.LoadScene("Gameplay");
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
        await serverManager.StartGame();
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
        serverManager.waitingForPlayers = true;
        lobbyText.text = serverManager.lobbyCode;

        lobbyCreation.SetActive(true);
        mainMenu.SetActive(false);
    }
}
