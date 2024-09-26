using Godot;
using System;

public partial class PushArea : Area2D
{
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        if(body is PushableStatue)
        {
            ((PushableStatue)body).pushDirection = GlobalPlayerManager.Instance.player.DirectionVector;
        }
        return;
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is PushableStatue)
        {
            ((PushableStatue)body).pushDirection = Vector2.Zero;
        }

        return;
    }
}
