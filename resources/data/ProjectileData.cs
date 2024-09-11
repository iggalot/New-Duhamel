using Godot;

public partial class ProjectileData : Node
{
    [Export]
    public CharacterBody2D ProjectileOwner { get; set; }
    // the speed of the projectile
    [Export]
    public  float ProjectileSpeed { get; set; }

    // the distance the projectile can travel
    [Export]
    public float ProjectileRangeDistance { get; set; }

    // the unit vector direction of the projectile
    [Export]
    public Vector2 ProjectileDirection { get; set; }

    // size of the projectile
    [Export]
    public float ProjectileSize { get; set; }

    public Vector2 ProjectileSpawnPosition { get; set; }

    public override string ToString()
    {
        string str = string.Empty;
        str += $"ProjectileOwner: {ProjectileOwner}\n";
        str += $"ProjectileSpeed: {ProjectileSpeed}\n";
        str += $"ProjectileRangeDistance: {ProjectileRangeDistance}\n";
        str += $"ProjectileDirection: {ProjectileDirection}\n";
        str += $"ProjectileSize: {ProjectileSize}\n";
        str += $"ProjectileSpawnPosition: {ProjectileSpawnPosition}\n";

        return str;
    }
}
