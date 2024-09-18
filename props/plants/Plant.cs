using Godot;
using System;

public partial class Plant : Node
{
    HitBox hitBox;
    public override void _Ready()
    {
        hitBox = GetNode<HitBox>("HitBox");
        hitBox.Damaged += TakeDamage;
    }

    public void TakeDamage(HurtBox hurtBox)
    {
        QueueFree();
        return;
    }
}
