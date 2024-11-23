using Godot;
using System;

public partial class BaseProjectile : CharacterBody2D
{
    private HurtBox hurt_box;

    // The target coordinate of the projectile
    public Vector2 target { get; set; }
    // Unit vector direction of the projectile
    public Vector2 direction { get; set; } = Vector2.Zero;
    // How long the projectile lasts in milliseconds
    public float duration { get; set; } = 500.0f;
    public float speed { get; set; } = 500.0f;
    public float damage { get; set; } = 10.0f;

    public override void _Ready()
    {
        // Set the hurtbox for this projectile
        hurt_box = GetNode<HurtBox>("HurtBox");
        hurt_box.damage = damage;
    }

    public override void _PhysicsProcess(double delta)
    {
        duration -= (float)delta;

        if(duration <= 0.0f)
        {
            QueueFree();
        }

        Velocity = direction.Normalized() * (float)(speed);
        GlobalPosition += Velocity * (float)delta;
    }
}
