using Godot;
using System;

public partial class DetonationAttack : CharacterBody2D
{
    private float duration = 2000.0f;
    private float timer = 500.0f;
    private float timerMax = 500.0f;
    private PlayerController player;
    private MonsterController owner;
    private HurtBox hurt_box;
    private CollisionShape2D collision_shape;
    private AnimationPlayer animation_player;

    float damage = 200.0f;
    float radius = 150.0f;

    public override void _Ready()
    {
        player = GlobalPlayerManager.Instance.player;
        hurt_box = GetNode<HurtBox>("HurtBox");
        hurt_box.damage = damage;
        collision_shape = hurt_box.GetNode<CollisionShape2D>("CollisionShape2D");
        ((CircleShape2D)collision_shape.Shape).Radius = radius;



        animation_player = GetNode<AnimationPlayer>("AnimationPlayer");

        Node node = GetParent<Node>();  // The abilities directory which is a subdirectory of the Monster Controller
        owner = node.GetParent<MonsterController>();

        GlobalPosition = owner.GlobalPosition;
    }

    public override async void _Process(double delta)
    {
        GlobalPosition = owner.GlobalPosition;

        float dist = owner.GlobalPosition.DistanceTo(player.GlobalPosition);
        // if position is close to the player, queue free
        if (dist < radius)
        {

            animation_player.Play("detonate");
            await ToSignal(animation_player, "animation_finished");

            if (owner.GlobalPosition.DistanceTo(player.GlobalPosition) < radius)
            {
                player.TakeDamage(hurt_box);
            }

            owner.TakeDamage(hurt_box);
            owner.QueueFree();

            return;
        }

        // update the timers
        duration--;
        timer--;
    }
}
