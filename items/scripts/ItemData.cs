using Godot;
using System;

public partial class ItemData : Resource
{
    [Export]
    public string ItemName { get; set; }
    [Export]
    public string ItemDescription { get; set; }
    [Export]
    public Texture2D ItemTexture { get; set; }



    public string ItemEffect { get; set; }
    [Export]
    public string ItemType { get; set; }
    [Export]
    public string ItemIcon { get; set; }
    [Export]
    public int ItemPrice { get; set; }
    [Export]
    public int ItemWeight { get; set; }
    [Export]
    public int ItemHitPoints
    { get; set; } = 100;
    [Export]
    public int ItemMaxStackSize { get; set; } = 1;

    public ItemData()
    {
        GD.Print("Item created");
    }
}
