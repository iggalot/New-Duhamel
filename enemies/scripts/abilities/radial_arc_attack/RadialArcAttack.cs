using Godot;
using System;

public partial class RadialArcAttack : Node2D
{

    // angle of the arc measured from 0 degrees which is the way the player is faceing
    [Export] float arc_begin_angle = -10f;
    [Export] float arc_end_angle = 10f;
    [Export] int number_of_rays = 10;
    private static string projectile_scene_path = "res://projectiles/base_projectile.tscn";
    private PackedScene projectile_scene = GD.Load<PackedScene>(projectile_scene_path);

    private float[] ray_angles;

    private PlayerController player;
    private MonsterController owner;
    private Node abilities_node;
    private Node projectiles_node;

    //private HurtBox hurt_box;

    public Vector2 direction { get; set; } = Vector2.Zero;
    [Export] float speed = 200.0f;
    [Export] float damage = 50.0f;
    [Export] private float duration = 3.0f;  // 60 frames per second...so 300 frames = 5 seconds
    private float timer = 0.50f;
    [Export] private float timerMax = 1.0f;

    private bool wasSet { get; set; } = false;

    public override void _Ready()
    {
        abilities_node = GetParent<Node>();  // The abilities directory
        MonsterController owner_node = (MonsterController)abilities_node.GetParent<Node>();
        projectiles_node = owner_node.GetNode<Node>("Projectiles");

        owner = abilities_node.GetParent<MonsterController>();
        player = GlobalPlayerManager.Instance.player;

        // create the array for the angle spread
        this.ray_angles = new float[number_of_rays];

        for (int i = 0; i < number_of_rays; i++)
        {
            this.ray_angles[i] = arc_begin_angle + ((arc_end_angle - arc_begin_angle) / (number_of_rays - 1) * i);
        }

        // In the event that the character hasn't been positioned or fully instantiated and the range attack fires, delete this attack
        if ((owner == null) || (owner.char_data == null) || (owner.char_data.PositionIsSet == false))
        {
            return;
        }
        else
        {
            wasSet = true;
        }


    }

    public override void _PhysicsProcess(double delta)
    {
        if(wasSet == true && owner == null){
            QueueFree();
            return;
        }

        timer -= (float)delta;

        if (timer <= 0.0f)
        {
            for (int i = 0; i < number_of_rays; i++)
            {
                if(owner != null)
                {
                    if (owner.CanSeePlayer is false)
                    {
                        // cant see the player so dont attack
                        return;
                    }

                    // now rotate the vector that targets the player by the ray_direction offset.
                    float angle = this.ray_angles[i] * (float)Math.PI / 180.0f;
                    Vector2 projectile_direction = ((player.GlobalPosition - owner.GlobalPosition).Normalized()).Rotated(angle);

                    BaseProjectile projectile = projectile_scene.Instantiate<BaseProjectile>();

                    projectile.GlobalPosition = owner.GlobalPosition;
                    projectile.direction = projectile_direction;
                    projectile.damage = damage;
                    projectile.speed = speed;
                    projectile.duration = duration;

                    projectiles_node.AddChild(projectile);
                }
            }

            timer = timerMax;
        }
    }
}
