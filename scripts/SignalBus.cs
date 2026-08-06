using System;
using Godot;

public static class SignalBus
{
    public static Action? StartGame;
    public static void BroadcastStartGame() => StartGame?.Invoke();
    public static Action? RestartGame;
    public static void BroadcastRestartGame() => RestartGame?.Invoke();
    public static Action? ReturnToMainMenu;
    public static void BroadcastReturnToMainMenu() => ReturnToMainMenu?.Invoke();
    public static Action? QuitGame;
    public static void BroadcastQuitGame() => QuitGame?.Invoke();
    public static Action<Acorn>? AcornGrabbed;
    public static void BroadcastAcornGrabbed(Acorn acorn) => AcornGrabbed?.Invoke(acorn);
    public static Action? GameOver;
    public static void BroadcastGameOver() => GameOver?.Invoke();
}
