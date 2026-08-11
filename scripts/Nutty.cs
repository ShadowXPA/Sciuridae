using System;
using Godot;

public partial class Nutty : Node3D
{
	private AnimationPlayer? _animationPlayer;

	public override void _Ready()
	{
		_animationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");
	}

	public void PlayAnimation(string animation)
	{
		if (_animationPlayer is null || _animationPlayer.IsPlaying() && _animationPlayer.CurrentAnimation == animation) return;

		GD.Print($"Request to play: {animation}");
		_animationPlayer?.Play(animation);
	}

	public void StopAnimation()
	{
		_animationPlayer?.Stop();
	}
}
