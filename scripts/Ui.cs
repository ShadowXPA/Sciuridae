using Godot;
using System;

public partial class Ui : Control
{
	public Label? Score { get; private set; }
	public Label? Highscore { get; private set; }
	public Label? Time { get; private set; }

	public override void _Ready()
	{
		Score = GetNode<Label>("%Score");
		Highscore = GetNode<Label>("%Highscore");
		Time = GetNode<Label>("%Time");
	}
}
