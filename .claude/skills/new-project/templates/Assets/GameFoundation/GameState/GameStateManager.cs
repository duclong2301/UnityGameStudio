using UnityEngine;

namespace {{PROJECT_NAMESPACE }}.GameFoundation.StateMachine
{
    /// <summary>
    /// Global application state machine. Static, event-driven. Subscribers listen to
    /// <see cref="OnGameStateChanged"/> and react — no polling, no direct coupling.
    /// </summary>
    public static class GameStateManager
{
    public delegate void OnGameStateChangedDelegate(GameState current, GameState last, object data);
    public static event OnGameStateChangedDelegate OnGameStateChanged;

    public static GameState CurrentState { get; private set; } = GameState.None;
    private static GameState _lastState = GameState.None;

    private static void Change(GameState next, object data = null)
    {
        if (CurrentState == next)
        {
            Debug.LogWarning($"[GameState] Attempted to change to same state: {next}");
            return;
        }
        _lastState = CurrentState;
        CurrentState = next;
        Debug.Log($"[GameState] {_lastState} → {next}");
        OnGameStateChanged?.Invoke(next, _lastState, data);
    }

    public static void Init(object data = null) => Change(GameState.Init, data);
    public static void Main(object data = null) => Change(GameState.Main, data);
    public static void Ready(object data = null) => Change(GameState.Ready, data);
    public static void Play(object data = null) => Change(GameState.Play, data);
    public static void Restart(object data = null) => Change(GameState.Restart, data);
    public static void Complete(object data = null) => Change(GameState.Complete, data);
    public static void GameOver(object data = null) => Change(GameState.GameOver, data);
    public static void Next(object data = null) => Change(GameState.Next, data);
    public static void WaitComplete(object data = null) => Change(GameState.WaitComplete, data);
    public static void WaitGameOver(object data = null) => Change(GameState.WaitGameOver, data);
    public static void Other(object data = null) => Change(GameState.Other, data);
    public static void Revice(object data = null) => Change(GameState.Revice, data);
}
}
