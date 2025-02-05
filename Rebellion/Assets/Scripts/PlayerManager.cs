using UnityEngine;

struct Player
{
    public int id;
    public string name;
    public int alignment;
    public int role;

    public Player(int id, string name, int alignment, int role)
    {
        this.id = id;
        this.name = name;
        this.alignment = alignment;
        this.role = role;
    }
}
public class PlayerManager : MonoBehaviour
{
    Player[] players;
    Player nullPlayer;

    private void Start()
    {
        players = new Player[15];
        nullPlayer = new Player(0, "", 0, 0);

        for (int i = 0; i < players.Length; ++i)
        {
            players[i] = nullPlayer;
        }
    }

    public void AddPlayer(string playerName)
    {
        int id = players.Length;

        players[id] = new Player(id, playerName, 0, 0);
    }

    public void RemovePlayer(int id)
    {
        players[id] = nullPlayer;
    }
}
