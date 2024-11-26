using Godot;
using System;

public partial class RangeAttack : Node
{
    private static string projectile_scene_path = "res://projectiles/base_projectile.tscn";

    private PlayerController player;
    private MonsterController owner;
    private Node abilities_node;
    private Node projectiles_node;

    //private HurtBox hurt_box;

    public Vector2 direction { get; set; } = Vector2.Zero;
    float speed = 100.0f;
    float damage = 10.0f;
    private float duration = 300.0f;  // 60 frames per second...so 300 frames = 5 seconds
    private float timer = 1.0f;
    private float timerMax = 1.0f;

    private bool wasSet { get; set; } = false;

    public override void _Ready()
    {
        abilities_node = GetParent<Node>();  // The abilities directory
        MonsterController owner_node = (MonsterController)abilities_node.GetParent<Node>();
        projectiles_node = owner_node.GetNode<Node>("Projectiles");

        owner = abilities_node.GetParent<MonsterController>();
        player = GlobalPlayerManager.Instance.player;


        // In the event that the character hasn't been positioned or fully instantiated and the range attack fires, delete this attack
        if ((owner == null) || (owner.char_data == null) || (owner.char_data.PositionIsSet == false))
        {
            return;
        } else
        {
            wasSet = true;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        timer -= (float)delta;
        if (timer <= 0.0f)
        {
            if (owner.CanSeePlayer is false)
            {
                // cant see the player so dont attack
                return;
            }

            //GD.Print("spawining projectile");
            // Create a new single projectile
            PackedScene projectile_scene = GD.Load<PackedScene>(projectile_scene_path);
            BaseProjectile projectile = projectile_scene.Instantiate<BaseProjectile>();

            Vector2 projectile_direction = (player.GlobalPosition - owner.GlobalPosition).Normalized();
            projectile.GlobalPosition = owner.GlobalPosition;

            projectile.direction = projectile_direction;
            projectile.damage = damage;
            projectile.speed = 200.0f;
            projectile.duration = 300.0f;

            projectiles_node.AddChild(projectile);

            timer = timerMax;
        }
    }
}
