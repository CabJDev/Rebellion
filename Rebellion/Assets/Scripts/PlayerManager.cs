using System.Collections.Generic;
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

    public Player[] players;
    public Player nullPlayer;

    public int playerCount = 15;
    public int currentPlayerCount = 0;

    private List<string> usedNames;

    public Dictionary<Player, int> selfAbilitesUsed;
    public List<Player> rebels;

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
        players = new Player[15];
        nullPlayer = new Player("", "", 0, 0, 0);
        nullPlayer.alive = false;

        usedNames = new List<string>();
        selfAbilitesUsed = new Dictionary<Player, int>();

        rebels = new List<Player>();

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
        for (int i = 0; i <= players.Length; ++i)
        {
            if (players[i].id == hash)
            {
                usedNames.Remove(players[i].name);
                players[i] = nullPlayer;
                currentPlayerCount--;
                break;
            }
        }
    }

    public void SetAlignment(string hash, int alignment) {
        int id = GetPlayerIndex(hash);
        players[id].alignment = alignment;

        if (alignment == 2) rebels.Add(players[GetPlayerIndex(hash)]);
    }

    public void SetRole(string hash, int role) {
        int id = GetPlayerIndex(hash);
        players[id].role = role; 
    }

    public int GetPlayerIndex(string hash)
    {
        for (int i = 0; i < players.Length; ++i)
            if (players[i].id == hash) return i;

        return -1;
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

    public void PrintPlayers()
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
    }
}
