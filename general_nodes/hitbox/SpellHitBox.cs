using Godot;
using System;

public partial class SpellHitBox : Area2D
{
    [Signal] public delegate void SpellDamagedEventHandler(SpellHurtBox spell_hurt_box, BaseSpell spell);

    public override void _Ready()
    {
    }

    public override void _Process(double delta)
    {
    }

    public void TakeSpellDamage(SpellHurtBox spell_hurt_box)
    {
        GD.Print("Emitting spell damage signal");
        Node spell_node = spell_hurt_box.GetParent();
        BaseSpell spell = spell_node as BaseSpell;

        //GD.Print( "TakeDamage: " + damage.ToString() );
        EmitSignal(SignalName.SpellDamaged, spell_hurt_box, spell);
    }
}
