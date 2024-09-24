using Godot;
using System;
using System.Collections.Generic;

public partial class ItemData : Resource
{
    // the maximum number of affects that can be assigned to an item
    private const int maxEffects = 5;

    [Export]
    public string ItemName { get; set; }
    [Export]
    public string ItemDescription { get; set; }
    [Export]
    public Texture2D ItemTexture { get; set; }



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



    [ExportCategory("Item Use Effects")]
    [Export] ItemEffect[] effects { get; set; } = new ItemEffect[maxEffects];

    public bool Use()
    {
        if(effects.Length == 0)
        {
            return false;
        }

        foreach (ItemEffect e in effects)
        {
            if(e != null)
            {
                e.Use();
            }
        }

        return true;
    }
}
