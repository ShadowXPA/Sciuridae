using System;
using Godot;

public partial class GameOver : Control
{
	public AnimationPlayer? AnimationPlayer { get; private set; }
	public Label? Highscore { get; private set; }
	private Button? _restart;
	private Button? _mainMenu;
	private Button? _quitBtn;

	public override void _Ready()
	{
		AnimationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");
		Highscore = GetNode<Label>("%Highscore");
		_restart = GetNode<Button>("%Restart");
		_mainMenu = GetNode<Button>("%MainMenu");
		_quitBtn = GetNode<Button>("%Quit");
		_restart.Pressed += RestartGame;
		_mainMenu.Pressed += GoToMainMenu;
		_quitBtn.Pressed += QuitGame;
	}

	public override void _ExitTree()
	{
		if (_restart is not null)
			_restart.Pressed -= RestartGame;
		if (_mainMenu is not null)
			_mainMenu.Pressed -= GoToMainMenu;
		if (_quitBtn is not null)
			_quitBtn.Pressed -= QuitGame;
	}

	public void RestartGame()
	{
		SignalBus.BroadcastRestartGame();
		// GetTree().ReloadCurrentScene();
	}

	public void GoToMainMenu()
	{
		SignalBus.BroadcastReturnToMainMenu();
		// GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
	}

	public void QuitGame()
	{
		SignalBus.BroadcastQuitGame();
	}
}
