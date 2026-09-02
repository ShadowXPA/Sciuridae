using System;
using Godot;

public partial class Nutty : Node3D
{
	public static readonly Vector2 IdleAnimation = Vector2.Up;
	public static readonly Vector2 RunAnimation = Vector2.Down;
	public static readonly Vector2 JumpAnimation = Vector2.Left;
	public static readonly Vector2 FallAnimation = Vector2.Right;

	private AnimationPlayer? _animationPlayer;
	private AnimationTree? _animationTree;

	public override void _Ready()
	{
		_animationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");
		_animationTree = GetNode<AnimationTree>("%AnimationTree");
	}

	public void PlayAnimation(string animation)
	{
		if (_animationPlayer is null || _animationPlayer.IsPlaying() && _animationPlayer.CurrentAnimation == animation) return;

		_animationPlayer.Play(animation);
	}

	public void StopAnimation()
	{
		_animationPlayer?.Stop();
	}

	public void TravelAnimation(string animation)
	{
		if (_animationTree is null) return;

		((AnimationNodeStateMachinePlayback)(GodotObject)_animationTree.Get("parameters/playback")).Travel(animation);
	}
}
