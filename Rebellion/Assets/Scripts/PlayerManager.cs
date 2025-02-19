using UnityEditor;
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

        for (int i = 0; i < players.Length; ++i)
            players[i] = nullPlayer;
    }

    public bool AddPlayer(string connectionID, string playerName)
    {

        for (int i = 0; i < players.Length; ++i)
        {
            if (players[i].id == "")
            {
                players[i] = new Player(connectionID, playerName, i + 1, 0, 0);
                players[i].alive = true;
                return true;
            }
        }

        return false;
    }

    public void RemovePlayer(int id) { players[id] = nullPlayer; }

    public void SetAlignment(int id, int alignment) { players[id].alignment = alignment; }

    public void SetRole(int id, int role) { players[id].role = role; }

    public bool IsPlayer(int id)
    {
        if (players[id].id == "") return false;
        else return true;
    }

    public bool IsAligned(int id)
    {
        if (players[id].alignment == 0) return false;
        else return true;
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
