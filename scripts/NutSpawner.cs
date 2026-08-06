using System;
using Godot;

public partial class NutSpawner : Node3D
{
	[ExportGroup("Spawner")]
	[Export]
	public Node? SpawnContainer { get; set; }
	[Export]
	public Timer? SpawnTimer { get; set; }
	[Export]
	public NavigationRegion3D? SpawnArea { get; set; }
	[ExportGroup("Nuts")]
	[Export]
	public PackedScene? NutScene { get; set; }
	[Export]
	public int MaxNuts { get; set; } = 50;

	public override void _Ready()
	{
		if (SpawnTimer is null || SpawnArea is null) return;

		SpawnTimer.Timeout += Spawn;
		SignalBus.StartGame += StartGame;
		SignalBus.RestartGame += StartGame;
		SignalBus.GameOver += GameOver;
	}

	public override void _ExitTree()
	{
		if (SpawnTimer is not null)
			SpawnTimer.Timeout -= Spawn;
		SignalBus.StartGame -= StartGame;
		SignalBus.RestartGame -= StartGame;
		SignalBus.GameOver -= GameOver;
	}

	private void StartGame()
	{
		SpawnTimer?.Start();
	}

	private void GameOver()
	{
		SpawnTimer?.Stop();

		if (SpawnContainer is not null)
			foreach (var child in SpawnContainer.GetChildren())
			{
				child.QueueFree();
			}
	}

	public Vector3 GetRandomSpawnPoint()
	{
		if (SpawnArea is null) return GlobalPosition;

		var aabb = SpawnArea.GetBounds();
		var map = SpawnArea.GetNavigationMap();

		return NavigationServer3D.MapGetClosestPoint(map, new Vector3(
			(float)GD.RandRange(aabb.Position.X, aabb.End.X),
			(float)GD.RandRange(aabb.Position.Y, aabb.End.Y),
			(float)GD.RandRange(aabb.Position.Z, aabb.End.Z)
		));
	}

	public void Spawn()
	{
		var container = SpawnContainer ?? this;

		if (container.GetChildCount() >= MaxNuts) return;

		var instance = NutScene!.Instantiate<Node3D>();
		container.AddChild(instance);
		instance.GlobalPosition = GetRandomSpawnPoint();
	}
}
