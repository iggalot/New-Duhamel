using Godot;
using System;
using static BaseSpell;

public partial class BaseSpellData : Node
{
    [Export] public string SpellName { get; set; }
    [Export] public float SpellRange { get; set; } = 250;
    [Export] public float SpellSpeed { get; set; } = 250;
    [Export] public Vector2 SpellDirection { get; set; } = new Vector2(1.0f, 0.0f);
    [Export] public float SpellDamage { get; set; } = 50.0f;

    [Export] public BaseSpell.SpellsNames SpellType { get; set; } = BaseSpell.SpellsNames.SPELL_LIGHTNING;

    public BaseSpellData()
    {
        Update();
    }

    public void Update()
    {
        SpellName = GetSpellName(SpellType);
    }

    public static string GetSpellName(SpellsNames spell)
    {
        switch (spell)
        {
            case SpellsNames.SPELL_FIREBALL:
                return "fireball";
            case SpellsNames.SPELL_POISONBALL:
                return "poisonball";
            case SpellsNames.SPELL_LIGHTNING:
                return "lightning";
            case SpellsNames.SPELL_ACIDARROW:
                return "acidarrow";
            case SpellsNames.SPELL_POISONSTREAM:
                return "poisonstream";
            case SpellsNames.SPELL_EARTH:
                return "earth";
            default:
                return "lightning";
        }
    }
}
