using Godot;
using System;

public partial class BaseSpellData : Node
{
    [Export] public string SpellName { get; set; } = "lightning";
    [Export] public float SpellRange { get; set; } = 250;
    [Export] public float SpellSpeed { get; set; } = 250;
    [Export] public Vector2 SpellDirection { get; set; } = new Vector2(1.0f, 0.0f);

    [Export] public BaseSpell.SpellsNames SpellType { get; set; } = BaseSpell.SpellsNames.SPELL_LIGHTNING;

}
