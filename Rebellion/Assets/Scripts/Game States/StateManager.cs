using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    [SerializeField]
    private List<IGameState> gameStates;

    [SerializeField]
    private GameStateViewer gameStateViewer;

    private ServerManager serverManager;

    private PlayerManager playerManager;

    private int currentState;

    public float currentStateLength;
    private int currentDay;

    [SerializeField] Material fog;

    private void Start()
    {
        playerManager = PlayerManager.Instance;
        serverManager = ServerManager.Instance;

        gameStates = new List<IGameState>();
        GetGameStates();

        currentState = 0;
        currentDay = 0;
        currentStateLength = 10;

        serverManager.currentState = currentState;

        gameStateViewer.SetDay(currentDay);
        gameStateViewer.SetPhase(gameStates[0].GetStateName());
        gameStateViewer.SetTime(currentStateLength);

        for (int i = 0; i < 15; i++)
        {
            Player player = playerManager.players[i];
            TMP_Text aliveModel = GameObject.Find("_PlayerModels/Alive/" + (i + 1) + "/Name").GetComponent<TMP_Text>();
			TMP_Text deadModel = GameObject.Find("_PlayerModels/Dead/" + (i + 1) + "/Name").GetComponent<TMP_Text>();

            if (player.id == "") continue;
            aliveModel.text = player.name;
            deadModel.text = player.name;
		}

        ShowAlivePlayer();
    }

    private void Update()
    {
		float currentFloat = fog.GetFloat("_DensityMultiplier");
		if (currentState == 2 && currentFloat < 0.1f)
			fog.SetFloat("_DensityMultiplier", Mathf.Lerp(currentFloat, 0.1f, 1 * Time.deltaTime));
        else if (currentState == 0 && currentFloat > 0.01f)
			fog.SetFloat("_DensityMultiplier", Mathf.Lerp(currentFloat, 0.01f, 1 * Time.deltaTime));


		if (serverManager.gameFinished) return;

        if (!serverManager.pauseGame) currentStateLength -= Time.deltaTime;
		if (!serverManager.pauseGame && currentStateLength <= 0.0f) EndState();
		else if (serverManager.pauseGame)
        {
			if (!serverManager.endDayEarly) return;
			serverManager.endDayEarly = false;
            serverManager.pauseGame = false;
			EndState();
		}

        if (serverManager.pauseGame)
            gameStateViewer.SetTime(serverManager.pauseGameTimer);
        else
			gameStateViewer.SetTime(currentStateLength);
	}

    private void EndState()
    {
		serverManager.CheckWinConditions();

        if (serverManager.gameFinished) return;

		gameStates[currentState].EndState();

		if (currentDay == 0 && currentState == 0)
			currentState = 2;
		else
			++currentState;

		if (currentState == gameStates.Count)
		{
			currentState = 0;
			++currentDay;

			gameStateViewer.SetDay(currentDay);
			Task sendMessage = serverManager.SystemMessage("Day " + currentDay);
		}

		serverManager.currentState = currentState;

		gameStates[currentState].Transition();
		currentStateLength = gameStates[currentState].GetStateLength();

		gameStateViewer.SetPhase(gameStates[currentState].GetStateName());
	}

    public void ShowAlivePlayer()
    {
        for (int i = 0; i < 15; i++)
        {
            Player player = playerManager.players[i];

            if (player.id == "" || !player.alive) continue;
            GameObject model = GameObject.Find("_PlayerModels/Alive/" + (i + 1));
            model.SetActive(true);
        }
    }

    public void HidePlayers()
    {
        for (int i = 0; i < 15; i++)
        {
            GameObject model = GameObject.Find("_PlayerModels/Alive/" + (i + 1));
            model.SetActive(false);
        }
    }

    public void ShowDeadPlayer(int index)
    {
		Player player = playerManager.players[index];

		if (player.id == "" || player.alive) return;
		GameObject model = GameObject.Find("_PlayerModels/Dead/" + (index + 1));
        if (model == null) return;
		model.SetActive(true);
	}

    private void GetGameStates()
    {
        gameStates.Add(gameObject.GetComponent<DiscussionState>());
        gameStates.Add(gameObject.GetComponent<TrialState>());
        gameStates.Add(gameObject.GetComponent<ActionsState>());
    }
}
