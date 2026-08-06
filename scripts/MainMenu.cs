using System;
using Godot;

public partial class MainMenu : Control
{

	private Button? _playBtn;
	private Button? _quitBtn;

	public override void _Ready()
	{
		_playBtn = GetNode<Button>("%Play");
		_quitBtn = GetNode<Button>("%Quit");

		_playBtn.Pressed += PlayPressed;
		_quitBtn.Pressed += QuitPressed;
	}

	public override void _ExitTree()
	{
		if (_playBtn is not null)
			_playBtn.Pressed -= PlayPressed;
		if (_quitBtn is not null)
			_quitBtn.Pressed -= QuitPressed;
	}

	private void PlayPressed()
	{
		SignalBus.BroadcastStartGame();
	}

	private void QuitPressed()
	{
		SignalBus.BroadcastQuitGame();
	}
}
