interface IGameState
{
    void Transition();
    void GameStateActions();
    void EndState();
    float GetStateLength();
}
