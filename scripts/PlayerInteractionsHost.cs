using Godot;
using System;

public partial class PlayerInteractionsHost : Node2D
{
    PlayerController player;
    public override void _Ready()
    {
        player = GetTree().Root.GetNode<PlayerController>("GameManager/PlayerController");
        player.DirectionChanged += UpdateDirection;
    }

    /// <summary>
    /// updates the rotation direction of the contents of this interaction node by changing the rotation of the parent
    /// </summary>
    /// <param name="new_direction"></param>
    public void UpdateDirection(Vector2 new_direction)
    {
        if (new_direction == Vector2.Down)
        {
            RotationDegrees = 0.0f;
        } else if (new_direction == Vector2.Up)
        {
            RotationDegrees = 180.0f;
        } else if (new_direction == Vector2.Left)
        {
            RotationDegrees = 90.0f;
        } else if (new_direction == Vector2.Right)
        {
            RotationDegrees = -90.0f;
        } else
        {
            RotationDegrees = 0.0f;
        }
        return;
    }
}
