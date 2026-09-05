using Godot;

public partial class Character : CharacterBody3D
{
	[ExportGroup("Camera")]
	[Export(PropertyHint.Range, "0.0, 1.0")]
	public float CameraSensitivity { get; set; } = .5f;
	[Export]
	public float TiltUpperLimit { get; set; } = Mathf.Pi / 2.1f;
	[Export]
	public float TiltLowerLimit { get; set; } = -Mathf.Pi / 2.1f;

	[ExportGroup("Movement")]
	[Export]
	public float Speed { get; set; } = 5.0f;
	[Export]
	public float Acceleration { get; set; } = 20.0f;
	[Export]
	public float RotationSpeed { get; set; } = 12.0f;
	[Export]
	public float JumpVelocity { get; set; } = 5.0f;
	[Export]
	public float SprintMultiplier { get; set; } = .75f;
	[Export]
	public Godot.Collections.Array<AudioStream> Footsteps { get; set; } = [];
	[Export]
	public double FootstepRate { get; set; } = 0.45;

	private Nutty? _nutty;
	private Vector3 _skinDefaultGlobalRotation;
	private Vector3 _cameraDefaultRotation;
	private Node3D? _cameraPivot;
	private Camera3D? _camera;
	private CollisionShape3D? _skin;
	private Vector2 _cameraInputDirection = Vector2.Zero;
	private Vector3 _lastMovementDirection = Vector3.Back;
	private Vector3 _defaultPosition = new(0, 0.6f, 0);
	private bool _inputEnabled = false;
	private AnimationPlayer? _animationPlayer;
	private AudioStreamPlayer? _footstepsSfx;
	private double _footstepTimer;
	private AudioStreamPlayer? _acornSfx;

	public override void _Ready()
	{
		_nutty = GetNode<Nutty>("%Nutty");
		_cameraPivot = GetNode<Node3D>("%CameraPivot");
		_camera = GetNode<Camera3D>("%Camera");
		_skin = GetNode<CollisionShape3D>("%Skin");
		_animationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");
		_footstepsSfx = GetNode<AudioStreamPlayer>("%FootstepsSFX");
		_acornSfx = GetNode<AudioStreamPlayer>("%AcornSFX");
		_cameraDefaultRotation = _cameraPivot.Rotation;
		_skinDefaultGlobalRotation = _skin.GlobalRotation;
	}

	public override void _Input(InputEvent @event)
	{
		if (!_inputEnabled) return;

		if (@event.IsActionPressed("left_click"))
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}

		if (@event.IsActionPressed("ui_cancel"))
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!_inputEnabled) return;

		if (@event is InputEventMouseMotion inputEventMouseMotion && Input.GetMouseMode() == Input.MouseModeEnum.Captured)
		{
			_cameraInputDirection = inputEventMouseMotion.ScreenRelative * CameraSensitivity;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_inputEnabled) return;

		_footstepTimer -= delta;

		var cameraPivotRotation = _cameraPivot!.Rotation;

		if (OS.HasFeature("mobile"))
		{
			var joystickInput = Input.GetVector(
				"camera_left",
				"camera_right",
				"camera_up",
				"camera_down"
			);

			cameraPivotRotation.X -= joystickInput.Y * (float)delta;
			cameraPivotRotation.X = Mathf.Clamp(cameraPivotRotation.X, TiltLowerLimit, TiltUpperLimit);
			cameraPivotRotation.Y -= joystickInput.X * (float)delta;
		}
		else
		{
			cameraPivotRotation.X -= _cameraInputDirection.Y * (float)delta;
			cameraPivotRotation.X = Mathf.Clamp(cameraPivotRotation.X, TiltLowerLimit, TiltUpperLimit);
			cameraPivotRotation.Y -= _cameraInputDirection.X * (float)delta;
		}

		_cameraPivot.Rotation = cameraPivotRotation;
		_cameraInputDirection = Vector2.Zero;

		Vector3 velocity = Velocity;
		Vector2 rawInput = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
		var sprinting = IsOnFloor() ? Input.GetActionRawStrength("sprint") : 0;
		var forward = _camera!.GlobalBasis.Z;
		var right = _camera.GlobalBasis.X;

		Vector3 moveDirection = forward * rawInput.Y + right * rawInput.X;
		moveDirection.Y = 0;
		moveDirection = moveDirection.Normalized();

		var yVelocity = velocity.Y;
		velocity.Y = 0;
		velocity = velocity.MoveToward(moveDirection * Speed * ((SprintMultiplier * sprinting) + 1.0f), Acceleration * (float)delta);
		velocity.Y = yVelocity;

		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		var isStartingJump = Input.IsActionJustPressed("jump") && IsOnFloor();
		if (isStartingJump)
		{
			velocity.Y += JumpVelocity;
		}

		Velocity = velocity;
		MoveAndSlide();

		if (moveDirection.Length() > 0.2f)
		{
			_lastMovementDirection = moveDirection;
		}

		var targetAngle = Vector3.Back.SignedAngleTo(_lastMovementDirection, Vector3.Up);
		var globalRotation = _skin!.GlobalRotation;
		globalRotation.Y = Mathf.LerpAngle(globalRotation.Y, targetAngle, RotationSpeed * (float)delta);
		_skin.GlobalRotation = globalRotation;

		if (isStartingJump)
		{
			_footstepsSfx?.Stop();
			// _footstepTimer = 0;

			// _nutty?.PlayAnimation("Jump");
			_nutty?.TravelAnimation("Jump");
		}
		else if (!IsOnFloor() && Velocity.Y < 0)
		{
			_footstepsSfx?.Stop();
			// _footstepTimer = 0;

			// _nutty?.PlayAnimation("Fall");
			_nutty?.TravelAnimation("Fall");
		}
		else if (IsOnFloor())
		{
			var groundSpeed = Velocity.Length();
			if (groundSpeed > 0)
			{
				if (_footstepsSfx is not null && _footstepTimer <= 0)
				{
					_footstepTimer = FootstepRate;
					PlayFootstep();
				}

				// _nutty?.PlayAnimation("Run");
				_nutty?.TravelAnimation("Run");
			}
			else
			{
				_footstepsSfx?.Stop();
				// _footstepTimer = 0;

				// _nutty?.PlayAnimation("Idle");
				_nutty?.TravelAnimation("Idle");
			}
		}
	}

	private void PlayFootstep()
	{
		if (_footstepsSfx is null || Footsteps.Count <= 0) return;

		_footstepsSfx.Stream = Footsteps[GD.RandRange(0, Footsteps.Count - 1)];
		_footstepsSfx.PitchScale = (float)GD.RandRange(0.9, 1.1);
		_footstepsSfx.Play();
	}

	public void RotateCamera()
	{
		_animationPlayer?.Play("rotate_camera");
	}

	public void Reset()
	{
		_animationPlayer?.Play("RESET");
		Velocity = Vector3.Zero;
		Position = _defaultPosition;
		if (_skin is not null)
			_skin.GlobalRotation = _skinDefaultGlobalRotation;
		if (_cameraPivot is not null)
			_cameraPivot.Rotation = _cameraDefaultRotation;
	}

	public void GrabAcorn(Acorn acorn)
	{
		_acornSfx?.Play();
		SignalBus.BroadcastAcornGrabbed(acorn);
	}

	public void SetInputEnabled(bool inputEnabled)
	{
		_inputEnabled = inputEnabled;
	}

	public void Stop()
	{
		SetInputEnabled(false);
		_nutty?.TravelAnimation("Idle");
	}
}
