using Godot;
using System;

public partial class GameOver : Control
{
	public AnimationPlayer? AnimationPlayer { get; private set; }
	public Label? Highscore { get; private set; }
	private Button? _restart;
	private Button? _mainMenu;

	public override void _Ready()
	{
		AnimationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");
		Highscore = GetNode<Label>("%Highscore");
		_restart = GetNode<Button>("%Restart");
		_mainMenu = GetNode<Button>("%MainMenu");
		_restart.Pressed += RestartGame;
		_mainMenu.Pressed += GoToMainMenu;
	}

    public void RestartGame()
    {
        GetTree().ReloadCurrentScene();
    }

    public void GoToMainMenu()
    {
        GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
    }
}
