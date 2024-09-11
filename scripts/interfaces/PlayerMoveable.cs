using Godot;
using System;

public class PlayerMoveable
{
    float speed = 300.0f;
    Vector2 motion = Vector2.Zero;

    /// <summary>
    /// checks the movement of the player from the input
    /// </summary>
    /// <returns></returns>
    public Vector2 HandleMovement()
    {
        var unit_vec = Vector2.Zero;

        if (Input.IsActionPressed("up"))
        {
            unit_vec.Y -= 1;
        }
        if (Input.IsActionPressed("down"))
        {
            unit_vec.Y += 1;
        }
        if (Input.IsActionPressed("right"))
        {
            unit_vec.X += 1;
        }
        if (Input.IsActionPressed("left"))
        {
            unit_vec.X -= 1;
        }

        return unit_vec; ;

    }

}
