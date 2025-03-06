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

		Role offensiveLoyalist = new Role("Vindicator", "Overzealous warriors tasked with the execution of rebels. If they mistakenly attack a fellow loyalist, they are executed.", "Attack", "All", "LoyalistsWins", "Eliminate rebels and offensive neutrals.");
		Role defensiveLoyalist = new Role("Oathbound", "The shield of the innocent. Oathbound warriors protects allies, and any that attack them are fated a swift death.", "Defend", "All", "LoyalistsWins", "Eliminate rebels and offensive neutrals.");
		Role supportiveLoyalist = new Role("Truthseeker", "Magical investigators working to expose rebels by tracking who they visit with their mystical orb.", "Track", "All", "LoyalistsWins", "Eliminate rebels and offensive neutrals.");

		loyalists[0] = offensiveLoyalist;
		loyalists[1] = defensiveLoyalist;
		loyalists[2] = supportiveLoyalist;

		Role offensiveRebel = new Role("Daggerfang", "Cold and calculating assassins that work to remove obstacles to the rebels' rise.", "Attack", "NonFaction", "RebelsWins", "Gain the majority of the voting power as a faction.");
		Role defensiveRebel = new Role("Mirage", "Masters of deception, focused on making it seem as if rebels did not visit anyone", "HideActions", "Faction", "RebelsWins", "Gain the majority of the voting power as a faction.");
		Role supportiveRebel = new Role("Silencer", "Magical disrupters focused on preventing players from being able to do anything for one night.", "Roleblock", "NonFaction", "RebelsWins", "Gain the majority of the voting power as a faction.");

		rebels[0] = offensiveRebel;
		rebels[1] = defensiveRebel;
		rebels[2] = supportiveRebel;

		Role offensiveNeutral = new Role("Madcap", "An unhinged and unpredictable killer that strikes at random targets.", "Attack", "All", "OnlySurvivor", "Kill everyone.");
		Role defensiveNeutral = new Role("Survivor", "Innocent bystander that seeks to stay alive until the end of the game.", "Defend", "Self+3", "Survives", "Survive no matter what.");
		Role supportiveNeutral = new Role("Bamboozler", "A master of social manipulation, focused on weaving lies and deceit their ultimate goal is to appear suspicious and be executed during trial.", "Defend", "Self+1", "Executed", "Be executed.");

		neutrals[0] = offensiveNeutral;
		neutrals[1] = defensiveNeutral;
		neutrals[2] = supportiveNeutral;

		roleList = new Role[][] { loyalists, rebels, neutrals };
	}

	public Role GetRole(int alignment, int role) { return roleList[alignment - 1][role - 1]; }
}
