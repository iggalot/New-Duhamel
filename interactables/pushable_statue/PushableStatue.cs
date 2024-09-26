using Godot;
using System;

public partial class PushableStatue : RigidBody2D
{
    private Vector2 _push_direction = Vector2.Zero;
    [Export] public float pushSpeed { get; set; } = 30.0f;

    public Vector2 pushDirection {
        get => _push_direction;
        set
        {
            SetPush(value);
        }
    }

    AudioStreamPlayer2D audio { get; set; }

    public override void _Ready()
    {
        audio = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
    }

    public override void _PhysicsProcess(double delta)
    {
        LinearVelocity = pushDirection * pushSpeed;
        return;
    }

    private void SetPush(Vector2 value)
    {
        _push_direction = value;
        if(pushDirection == Vector2.Zero)
        {
            audio.Stop();
        } 
        else
        {
            audio.Play();
        }

        GD.Print("object is being pushed in direction: " + _push_direction);


    }
}
