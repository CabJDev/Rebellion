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
        gameStateViewer.SetPhase(gameStates[0].GetStateName());
        gameStateViewer.SetTime(currentStateLength);

        foreach (Player player in playerManager.players)
        {
            if (player.id != "")
            {
                GameObject playerModel = alivePlayerModels.Find(player.position.ToString()).gameObject;
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

            if (currentDay == 0 && currentState == 0)
                currentState = 2;
            else
                ++currentState;

            if (currentState == gameStates.Count)
            {
                currentState = 0;
                ++currentDay;

                gameStateViewer.SetDay(currentDay);
            }

            gameStates[currentState].Transition();
            currentStateLength = gameStates[currentState].GetStateLength();

            gameStateViewer.SetPhase(gameStates[currentState].GetStateName());
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
