namespace {{PROJECT_NAMESPACE}}.GameFoundation.StateMachine
{
    public enum GameState
    {
        None         = 0,
        Init         = 1,
        Main         = 2,
        Ready        = 3,
        Play         = 4,
        Restart      = 5,
        Complete     = 6,
        GameOver     = 7,
        Next         = 8,
        WaitComplete = 9,
        WaitGameOver = 10,
        Other        = 11,
        Revice       = 12,
    }
}
