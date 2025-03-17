using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ActionsState : MonoBehaviour, IGameState
{
    ServerManager serverManager;
    PlayerManager playerManager;
    RoleManager roleManager;

    public float GetStateLength() { return 40; }
    public string GetStateName() { return "Actions"; }

    private Dictionary<Player, List<Player>> defendedPlayers = new Dictionary<Player, List<Player>>();
    private Dictionary<Player, List<Player>> attackedPlayers = new Dictionary<Player, List<Player>>();
    private Dictionary<Player, List<Player>> hiddenActions = new Dictionary<Player, List<Player>>();
    private Dictionary<Player, List<Player>> roleBlocked = new Dictionary<Player, List<Player>>();
    private Dictionary<Player, List<Player>> trackedPlayers = new Dictionary<Player, List<Player>>();

    [SerializeField] StateManager stateManager;
    [SerializeField] GameObject[] lights;

    private void Start() 
    {
        serverManager = ServerManager.Instance;
        playerManager = PlayerManager.Instance;    
        roleManager = RoleManager.Instance;
    }

	public void GameStateActions()
    {

    }

    public void EndState()
    {
        Task buttonsDisabled = serverManager.DisableButtons();

        defendedPlayers.Clear();
        attackedPlayers.Clear();
        hiddenActions.Clear();
        roleBlocked.Clear();
        trackedPlayers.Clear();

        foreach (string playerHash in serverManager.targets.Keys)
        {
            Player player = playerManager.GetPlayer(playerHash);
            Role role = roleManager.GetRole(player.alignment, player.role);
            string actionType = role.roleActions;

            Player target = playerManager.GetPlayer(serverManager.targets[playerHash]);

            if (actionType == "Attack")
            {
                if (!attackedPlayers.ContainsKey(target))
					attackedPlayers.Add(target, new List<Player>());
				attackedPlayers[target].Add(player);
			}
            else if (actionType == "Defend")
            {
                if (!defendedPlayers.ContainsKey(target))
				    defendedPlayers.Add(target, new List<Player>());
                defendedPlayers[target].Add(player);
			}
            else if (actionType == "HideActions")
            {
                if (!hiddenActions.ContainsKey(target))
					hiddenActions.Add(target, new List<Player>());
                hiddenActions[target].Add(player);
			}
            else if (actionType == "Roleblock")
            {
                if (!roleBlocked.ContainsKey(target))
				    roleBlocked.Add(target, new List<Player>());
                roleBlocked[target].Add(player);
                Task systemMsg = serverManager.PlayerSystemMessage(target.id, "You have been roleblocked!");
			}
            else if (actionType == "Track")
            {
                if (!trackedPlayers.ContainsKey(target))
					trackedPlayers.Add(target, new List<Player>());
                trackedPlayers[target].Add(player);
			}
        }

        foreach (Player attackedPlayer in attackedPlayers.Keys)
        {
            bool defended = false;
            List<Player> attackers = attackedPlayers[attackedPlayer];

            if (defendedPlayers.ContainsKey(attackedPlayer))
            {
                foreach (Player defender in defendedPlayers[attackedPlayer])
                {
                    if (roleBlocked.ContainsKey(defender)) continue;
                    defended = true;
					Task systemMsg = serverManager.PlayerSystemMessage(defender.id, "Someone attacked your target!");
				}
            }

            if (defended)
            {
				Task systemMsg = serverManager.PlayerSystemMessage(attackedPlayer.id, "You were attacked last night, but you were protected!");
                continue;
			}

            List<Player> successfulAttackers = new List<Player>();

            foreach (Player attacker in attackers)
            {
                if (roleBlocked.ContainsKey(attacker)) continue;
                successfulAttackers.Add(attacker);
            }

            if (successfulAttackers.Count == 0) continue;

            Task playerKilled = serverManager.PlayerKilled(attackedPlayer);

            foreach (Player attacker in successfulAttackers)
            {
				if (attacker.alignment == 1 && attackedPlayer.alignment == 1)
				{
                    if (!serverManager.recentlyDead.Contains(attacker))
                    {
						Task killerKilled = serverManager.PlayerKilled(attacker);
					}
					Task systemMsg = serverManager.PlayerSystemMessage(attacker.id, "You attacked a fellow loyalist! The power within you burns you to a crisp.");
				}
			}
        }

        foreach (Player trackedPlayer in trackedPlayers.Keys)
        {
            if (trackedPlayer.alignment == 1 && trackedPlayer.role == 3) continue;

            bool tracked = true;
            List<Player> trackers = trackedPlayers[trackedPlayer];

			if (hiddenActions.ContainsKey(trackedPlayer))
			{
				foreach (Player hider in hiddenActions[trackedPlayer])
				{
					if (roleBlocked.ContainsKey(hider)) continue;
					tracked = false;
				}
			}

            if (!tracked) continue;

            List<Player> successfulTrackers = new List<Player>();

			foreach (Player tracker in trackers)
			{
				if (roleBlocked.ContainsKey(tracker)) continue;
				successfulTrackers.Add(tracker);
			}

            if (successfulTrackers.Count == 0) continue;

			string visitedPlayerHash = "";
            if (serverManager.targets.ContainsKey(trackedPlayer.id))
                visitedPlayerHash = serverManager.targets[trackedPlayer.id];

            foreach (Player tracker in successfulTrackers)
            {
				if (visitedPlayerHash == "")
				{
					Task systemMsg = serverManager.PlayerSystemMessage(tracker.id, $"{trackedPlayer.name} stayed at home!");
				}
				else
				{
					Player visitedPlayer = playerManager.GetPlayer(visitedPlayerHash);
					Task systemMsg = serverManager.PlayerSystemMessage(tracker.id, $"{trackedPlayer.name} visited {visitedPlayer.name} last night!");
				}
			}
        }

        for (int i = 0; i < serverManager.recentlyDead.Count; i++)
            playerManager.players[playerManager.GetPlayerIndex(serverManager.recentlyDead[i].id)].alive = false;

		foreach (GameObject light in lights)
			light.SetActive(true);
	}

    public void Transition()
    {
        serverManager.targets = new Dictionary<string, string>();
        Task buttonsEnabled = serverManager.EnableButtons();

        defendedPlayers.Clear();
        attackedPlayers.Clear();
        hiddenActions.Clear();
        roleBlocked.Clear();
        trackedPlayers.Clear();

        stateManager.HidePlayers();

        foreach (GameObject light in lights)
            light.SetActive(false);

		GameStateActions();
    }
}
