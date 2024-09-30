using Godot;
using System;

public partial class BaseSpellData : Node
{
    [Export] public string SpellName { get; set; }
    [Export] public float SpellRange { get; set; } = 250;
    [Export] public float SpellSpeed { get; set; } = 250;
    [Export] public Vector2 SpellDirection { get; set; } = new Vector2(0.7071067811865476f, -0.7071067811865476f);


}
