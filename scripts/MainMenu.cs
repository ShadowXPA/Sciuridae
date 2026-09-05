using System;
using Godot;

public partial class MainMenu : Control
{
	private Button? _playBtn;
	private Button? _optionsBtn;
	private Button? _creditsBtn;
	private Button? _quitBtn;

	public override void _Ready()
	{
		_playBtn = GetNode<Button>("%Play");
		_optionsBtn = GetNode<Button>("%Options");
		_creditsBtn = GetNode<Button>("%Credits");
		_quitBtn = GetNode<Button>("%Quit");

		_playBtn.Pressed += PlayPressed;
		_optionsBtn.Pressed += OptionsPressed;
		_creditsBtn.Pressed += CreditsPressed;
		_quitBtn.Pressed += QuitPressed;
	}

	public override void _ExitTree()
	{
		if (_playBtn is not null)
			_playBtn.Pressed -= PlayPressed;
		if (_optionsBtn is not null)
			_optionsBtn.Pressed -= OptionsPressed;
		if (_creditsBtn is not null)
			_creditsBtn.Pressed -= CreditsPressed;
		if (_quitBtn is not null)
			_quitBtn.Pressed -= QuitPressed;
	}

	private void PlayPressed()
	{
		SignalBus.BroadcastStartGame();
	}

	private void OptionsPressed()
	{
		SignalBus.BroadcastOptionsMenu(true);
	}

	private void CreditsPressed()
	{
		SignalBus.BroadcastCreditsMenu(true);
	}

	private void QuitPressed()
	{
		SignalBus.BroadcastQuitGame();
	}
}
