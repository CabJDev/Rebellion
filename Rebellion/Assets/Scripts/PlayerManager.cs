using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public struct Player
{
    public string id;
    public string name;
    public int position;
    public int alignment;
    public int role;
    public bool alive;

    public Player(string id, string name, int position, int alignment, int role)
    {
        this.id = id;
        this.name = name;
        this.position = position;
        this.alignment = alignment;
        this.role = role;
        this.alive = true;
    }
}

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    private ServerManager serverManager;

    public Player[] players;
    public Player nullPlayer;

    public int playerCount = 15;
    public int currentPlayerCount = 0;

    private List<string> usedNames;

    public Dictionary<Player, int> selfAbilitesUsed;
    public List<string> rebels;
    public List<string> loyalists;
    public List<string> offensiveRebels;
    public List<string> offensiveNeutrals;

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

    private void Start()
    {
        serverManager = ServerManager.Instance;

        players = new Player[15];
        nullPlayer = new Player("", "", 0, 0, 0);
        nullPlayer.alive = false;

        usedNames = new List<string>();
        selfAbilitesUsed = new Dictionary<Player, int>();

        rebels = new List<string>();

        for (int i = 0; i < players.Length; ++i)
            players[i] = nullPlayer;
    }

    public bool AddPlayer(string hash, string playerName)
    {
        if (usedNames.Contains(playerName)) return false;

        for (int i = 0; i < players.Length; ++i)
        {
            if (players[i].id == "")
            {
                usedNames.Add(playerName);

                players[i] = new Player(hash, playerName, i + 1, 0, 0);
                players[i].alive = true;
                currentPlayerCount++;

                return true;
            }
        }

        return false;
    }

    public void RemovePlayer(string hash)
    {
        int playerIndex = GetPlayerIndex(hash);

		usedNames.Remove(players[playerIndex].name);
        if (serverManager.gameStarted)
        {
			Task killPlayer = serverManager.PlayerKilled(players[playerIndex]);
		}
        else   
		    players[playerIndex] = nullPlayer;

		currentPlayerCount--;
	}

    public void SetAlignment(string hash, int alignment) {
        int id = GetPlayerIndex(hash);
        players[id].alignment = alignment;

        if (alignment == 2) rebels.Add(hash);
        if (alignment == 1) loyalists.Add(hash);
    }

    public void SetRole(string hash, int role) {
        int id = GetPlayerIndex(hash);
        players[id].role = role; 

        if (players[id].alignment == 3 && players[id].role == 1)
            offensiveNeutrals.Add(hash);
        else if (players[id].alignment == 3 && players[id].role == 2)
            serverManager.winners.Add(players[id].id);
        else if (players[id].alignment == 2 &&  players[id].role == 1)
            offensiveRebels.Add(hash);
    }

    public int GetPlayerIndex(string hash)
    {
        for (int i = 0; i < players.Length; ++i)
            if (players[i].id == hash) return i;

        return -1;
    }

    public Player GetPlayer(string hash)
    {
        for (int i = 0; i < players.Length; ++i)
            if (players[i].id == hash) return players[i];

        return nullPlayer;
    }

    public bool IsPlayer(int index)
    {
        return players[index].id != "";
    }

    public bool IsAligned(string hash)
    {
        int id = GetPlayerIndex(hash);

        return players[id].alignment != 0;
    }

    /*public void PrintPlayers()
    {
        int rebelAmount = 0;
        int neutralAmount = 0;
        int loyalistAmount = 0;

        for (int i = 0; i < players.Length; ++i)
        {
            if (players[i].id != "")
            {
                string alignment = "";
                string role = "";

                switch (players[i].alignment)
                {
                    case 0:
                        alignment = "null";
                        break;
                    case 1:
                        alignment = "Loyalist";
                        loyalistAmount++;
                        break;
                    case 2:
                        alignment = "Rebel";
                        rebelAmount++;
                        break;
                    case 3:
                        alignment = "Neutral";
                        neutralAmount++;
                        break;
                }

                switch (players[i].role)
                {
                    case 0:
                        role = "null";
                        break;
                    case 1:
                        role = "Offensive";
                        break;
                    case 2:
                        role = "Defensive";
                        break;
                    case 3:
                        role = "Supportive";
                        break;
                }

                Debug.Log(players[i].name + " | Alignment: " + alignment + " | Role: " + role );
            }
        }

        Debug.Log("There are: " + loyalistAmount + " loyalists, " +  rebelAmount + " rebels and " + neutralAmount + " neutrals");
    }*/
}
