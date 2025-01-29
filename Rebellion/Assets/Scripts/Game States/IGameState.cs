interface IGameState
{
    void Transition();
    void GameStateActions();
    float GetStateLength();
}
