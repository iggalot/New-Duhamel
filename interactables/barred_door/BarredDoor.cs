using Godot;
using System;

public partial class BarredDoor : Node2D
{
    AnimationPlayer animationPlayer { get; set; }

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public void OnOpenDoor()
    {
        animationPlayer.Play("open_door");
    }

    public void OnCloseDoor()
    {
        animationPlayer.Play("close_door");

    }
}
