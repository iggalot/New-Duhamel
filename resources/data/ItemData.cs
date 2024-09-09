using Godot;
using System;

public partial class ItemData : Node
{
    [Export]
    public string Name;
    [Export]
    public string Effect;
    [Export]
    public string ItemType;
    [Export]
    public string Description { get; set; }
    [Export]
    public string Icon;
    [Export]
    public int Price;
    [Export]
    public int Weight;
    [Export]
    public int Durability { get; set; } = 100;
    [Export]
    public int MaxStackSize { get; set; } = 1;

    public ItemData()
    {
        GD.Print("Item created");
    }
}
