using Godot;

public partial class ProjectileData : Node
{
    // the speed of the projectile
    [Export]
    public  float Speed { get; set; }

    // the distance the projectile can travel
    [Export]
    public float RangeDistance { get; set; }

    // the unit vector direction of the projectile
    [Export]
    public Vector2 Direction { get; set; }

    // size of the projectile
    [Export]
    public float ProjectileSize { get; set; }
}
