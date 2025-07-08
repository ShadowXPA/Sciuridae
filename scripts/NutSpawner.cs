using Godot;
using System;

public partial class NutSpawner : Node3D
{
	[ExportGroup("Spawner")]
	[Export]
	public Node? SpawnContainer { get; set; }
	[Export]
	public Timer? SpawnTimer { get; set; }
	[Export]
	public CollisionShape3D? SpawnArea { get; set; }
	[ExportGroup("Nuts")]
	[Export]
	public PackedScene? NutScene { get; set; }
	[Export]
	public int MaxNuts { get; set; } = 50;

	private int _numberOfNuts;

	public override void _Ready()
	{
		if (SpawnTimer is null || SpawnArea is null) return;

		SpawnTimer.Timeout += Spawn;
	}

	public Vector3 GetRandomSpawnPoint()
	{
		if (SpawnArea is null) return GlobalPosition;

		var aabb = SpawnArea.Shape.GetDebugMesh().GetAabb();

		Vector3 randomLocal = new Vector3(
			(float)GD.RandRange(aabb.Position.X, aabb.End.X),
			(float)GD.RandRange(aabb.Position.Y, aabb.End.Y),
			(float)GD.RandRange(aabb.Position.Z, aabb.End.Z)
		);

		return SpawnArea.GlobalTransform * randomLocal;
	}

	public void Spawn()
	{
		GD.PrintS("Current number of nuts:", _numberOfNuts);
		if (_numberOfNuts >= MaxNuts) return;
		_numberOfNuts++;

		var container = SpawnContainer ?? this;

		var instance = NutScene!.Instantiate<Node3D>();
		instance.TreeExited += () => _numberOfNuts--;
		container.AddChild(instance);
		instance.GlobalPosition = GetRandomSpawnPoint();
	}
}
