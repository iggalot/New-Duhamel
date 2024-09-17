using Godot;

public partial class HurtBox : Area2D
{
    [Export] public float damage = 1;

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
    }

    public override void _Process(double delta)
    {
    }

    public void OnAreaEntered(Area2D area)
    {
        if(area is HitBox)
        {
            ((HitBox)area).TakeDamage(damage);
        }
        return;
    }
}
