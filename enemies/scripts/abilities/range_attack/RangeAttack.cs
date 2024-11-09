using Godot;
using System;

public partial class RangeAttack : CharacterBody2D
{
    private float duration = 2000.0f;
    private float timer = 500.0f;
    private float timerMax = 500.0f;
    private PlayerController player;
    private MonsterController owner;
    private HurtBox hurt_box;

    Vector2 direction = Vector2.Zero;
    float speed = 100.0f;
    float damage = 10.0f;

    public override void _Ready()
    {
        player = GlobalPlayerManager.Instance.player;
        hurt_box = GetNode<HurtBox>("HurtBox");
        hurt_box.damage = damage;

        Node node = GetParent<Node>();  // The abilities directory
        owner = node.GetParent<MonsterController>();

        direction = (player.GlobalPosition - owner.GlobalPosition).Normalized();

        Position = owner.GlobalPosition;

    }

    public override void _Process(double delta)
    {
        float dist = GlobalPosition.DistanceTo(player.GlobalPosition);
        // if position is close to the player, queue free
        if(dist < 16.0f)
        {
            //player.TakeDamage(hurt_box);
            GD.Print("--- damage...so delete the object");
            QueueFree();
            GD.Print("--- deleting object");


            return;
        } 


        Velocity = direction.Normalized() * (float)(speed);

        // The life span of ranged attack as a timer -- delete the object after this timer expires
        if(duration <= 0.0f)
        {
            QueueFree();
            return;
        }

        Position += Velocity * (float)delta;

        // update the timers
        duration--;
        timer--;
    }
}
