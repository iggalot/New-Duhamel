using Godot;
using System;

public partial class HitBox : Area2D
{
    [Signal] public delegate void DamagedEventHandler(float damage);

    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
    }

    public void TakeDamage(float damage) 
    { 
        GD.Print( "TakeDamage: " + damage.ToString() );
        EmitSignal(SignalName.Damaged, damage);
    }
}
