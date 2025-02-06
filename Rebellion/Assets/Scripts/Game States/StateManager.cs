using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    [SerializeField]
    private List<IGameState> gameStates;

    [SerializeField]
    private GameStateViewer gameStateViewer;

    [SerializeField]
    private ServerManager serverManager;

    [SerializeField]
    private Transform alivePlayerModels;
    [SerializeField]
    private Transform deadPlayerModels;

    private PlayerManager playerManager;

    private int currentState;

    private float currentStateLength;
    private int currentDay;

    private void Start()
    {
        playerManager = PlayerManager.Instance;

        gameStates = new List<IGameState>();
        GetGameStates();

        currentState = 0;
        currentDay = 0;
        currentStateLength = gameStates[0].GetStateLength();

        gameStateViewer.SetDay(currentDay);
        gameStateViewer.SetPhase(gameStates[0].GetType().ToString());
        gameStateViewer.SetTime(currentStateLength);

        foreach (Player player in playerManager.players)
        {
            if (player.id != 0)
            {
                GameObject playerModel = alivePlayerModels.Find(player.id.ToString()).gameObject;
                playerModel.SetActive(true);
            }
        }

        for (int i = 0; i < gameStates.Count; i++)
        {
            if (playerManager.IsPlayer(i + 1))
            {
                GameObject playerModel = alivePlayerModels.Find(playerManager.players[i + 1].id.ToString()).gameObject;
                playerModel.SetActive(true);
            }
            else
            {
                GameObject playerModel = deadPlayerModels.Find(playerManager.players[i + 1].id.ToString()).gameObject;
                playerModel.SetActive(true);
            }
        }
    }

    private void Update()
    {
        currentStateLength -= Time.deltaTime;
        if (currentStateLength <= 0.0f)
        {
            gameStates[currentState].EndState();

            ++currentState;

            if (currentState == gameStates.Count)
            {
                currentState = 0;
                ++currentDay;

                gameStateViewer.SetDay(currentDay);
            }

            gameStates[currentState].Transition();
            currentStateLength = gameStates[currentState].GetStateLength();

            gameStateViewer.SetPhase(gameStates[currentState].GetType().ToString());
        }
        gameStateViewer.SetTime(currentStateLength);
    }

    private void GetGameStates()
    {
        gameStates.Add(gameObject.GetComponent<DiscussionState>());
        gameStates.Add(gameObject.GetComponent<TrialState>());
        gameStates.Add(gameObject.GetComponent<ActionsState>());
    }
}
