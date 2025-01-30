using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    [SerializeField]
    private List<IGameState> gameStates;

    [SerializeField]
    private GameStateViewer gameStateViewer;

    [SerializeField]
    ServerManager serverManager;

    private int currentState;

    private float currentStateLength;
    private int currentDay;

    private void Start()
    {
        gameStates = new List<IGameState>();
        GetGameStates();

        currentState = 0;
        currentDay = 0;
        currentStateLength = gameStates[0].GetStateLength();

        gameStateViewer.SetDay(currentDay);
        gameStateViewer.SetPhase(gameStates[0].GetType().ToString());
        gameStateViewer.SetTime(currentStateLength);
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
