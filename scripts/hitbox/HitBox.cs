using Godot;
using System;

public partial class HitBox : Area2D
{
    [Signal] public delegate void DamagedEventHandler(HurtBox hurt_box);

    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
    }

    public void TakeDamage(HurtBox hurt_box) 
    { 
        //GD.Print( "TakeDamage: " + damage.ToString() );
        EmitSignal(SignalName.Damaged, hurt_box);
    }
}
