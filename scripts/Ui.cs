using System;
using Godot;

public partial class Ui : Control
{
	[Export]
	public float JoystickLookSpeed = 500.0f;

	public Label? Score { get; private set; }
	public Label? Highscore { get; private set; }
	public Label? Time { get; private set; }

	private VirtualJoystick? _moveJoystick;
	private VirtualJoystick? _cameraJoystick;
	private TextureButton? _jumpBtn;

	public override void _Ready()
	{
		Score = GetNode<Label>("%Score");
		Highscore = GetNode<Label>("%Highscore");
		Time = GetNode<Label>("%Time");

		_moveJoystick = GetNode<VirtualJoystick>("%MoveJoystick");
		_cameraJoystick = GetNode<VirtualJoystick>("%CameraJoystick");
		_jumpBtn = GetNode<TextureButton>("%JumpButton");

		_jumpBtn.Pressed += JumpPressed;

		if (!OS.HasFeature("mobile"))
		{
			_moveJoystick.Hide();
			_cameraJoystick.Hide();
			_jumpBtn.Hide();
		}
	}

	public override void _ExitTree()
	{
		if (_jumpBtn is not null)
			_jumpBtn.Pressed -= JumpPressed;
	}

	private void JumpPressed()
	{
		Input.ActionPress("jump");
		Input.ActionRelease("jump"); // I don't know... needs to be here
	}
}
