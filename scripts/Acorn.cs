using Godot;

public partial class Acorn : Node3D
{
	public int Score { get; set; } = 1;
	private Area3D? _pickupArea;

	public override void _Ready()
	{
		_pickupArea = GetNode<Area3D>("%PickupArea");
		_pickupArea.BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node3D body)
	{
		if (body is not Character character) return;

		if (_pickupArea is not null)
			_pickupArea.BodyEntered -= OnBodyEntered;

		character.GrabAcorn(this);

		QueueFree();
	}
}
