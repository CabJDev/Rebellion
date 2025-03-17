using UnityEngine;

public struct Role
{
	public string roleName;
	public string roleDesc;
	public string roleActions;
	public string targets;
	public string winCondition;
	public string winConditionDesc;

	public Role(string roleName, string roleDesc, string roleActions, string targets, string winCondition, string winConditionDesc)
	{
		this.roleName = roleName;
		this.roleDesc = roleDesc;
		this.roleActions = roleActions;
		this.targets = targets;
		this.winCondition = winCondition;
		this.winConditionDesc = winConditionDesc;
	}
}

public class RoleManager : MonoBehaviour
{
    public static RoleManager Instance;

	private Role[][] roleList;

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

	public void Start()
	{
		/*
		 * Alignment + Role Lists
		 * 0 = null
		 * 1 = Loyalists (Offensive, Defensive
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

		Role[] loyalists = new Role[3];
		Role[] rebels = new Role[3];
		Role[] neutrals = new Role[3];

		Role offensiveLoyalist = new Role("Loyalist Vindicator", "Select a target to attack at night. If you kill a fellow loyalist, the power within you burns you into ashes.", "Attack", "All", "LoyalistsWins", "Eliminate rebels and offensive neutrals.");
		Role defensiveLoyalist = new Role("Loyalist Oathbound", "Select a target to protect from all attacks at night.", "Defend", "All", "LoyalistsWins", "Eliminate rebels and offensive neutrals.");
		Role supportiveLoyalist = new Role("Loyalist Truthseeker", "Select a target to see who they visit at night. You are untrackable by other trackers.", "Track", "All", "LoyalistsWins", "Eliminate rebels and offensive neutrals.");

		loyalists[0] = offensiveLoyalist;
		loyalists[1] = defensiveLoyalist;
		loyalists[2] = supportiveLoyalist;

		Role offensiveRebel = new Role("Rebel Daggerfang", "Select a target to kill at night.", "Attack", "NonFaction", "RebelsWins", "Gain the majority of the voting power with other rebels.");
		Role defensiveRebel = new Role("Rebel Mirage", "Select a fellow rebel to hide from the Truthseeker's vision at night.", "HideActions", "Faction", "RebelsWins", "Gain the majority of the voting power with other rebels.");
		Role supportiveRebel = new Role("Rebel Silencer", "Select a target to prevent them from using their role abilities at night.", "Roleblock", "NonFaction", "RebelsWins", "Gain the majority of the voting power with other rebels.");

		rebels[0] = offensiveRebel;
		rebels[1] = defensiveRebel;
		rebels[2] = supportiveRebel;

		Role offensiveNeutral = new Role("Neutral Madcap", "Select a target to kill at night.", "Attack", "All", "OnlySurvivor", "Kill everyone.");
		Role defensiveNeutral = new Role("Neutral Survivor", "Select yourself as a target to defend yourself at night. You can only do this three times.", "Defend", "Self+3", "Survives", "Survive no matter what.");
		Role supportiveNeutral = new Role("Neutral Bamboozler", "Select yourself as a target to defend yourself at night. You can only do this once.", "Defend", "Self+1", "Executed", "Be executed.");

		neutrals[0] = offensiveNeutral;
		neutrals[1] = defensiveNeutral;
		neutrals[2] = supportiveNeutral;

		roleList = new Role[][] { loyalists, rebels, neutrals };
	}

	public Role GetRole(int alignment, int role) { return roleList[alignment - 1][role - 1]; }
}
