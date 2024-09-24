using Godot;
using System;

public partial class DropData : Resource
{
    [Export] public ItemData item { get; set; }
    [Export(PropertyHint.Range, "0, 100, 1, suffix:%")] public float probability { get; set; } = 100.0f;
    [Export(PropertyHint.Range, "0, 10, 1, suffix:items")] public int minAmount { get; set; } = 1;
    [Export(PropertyHint.Range, "0, 10, 1, suffix:items")] public int maxAmount { get; set; } = 1;

    public int GetDropCount()
    {
        var rng = new RandomNumberGenerator();
        if(rng.RandfRange(0.0f, 100.0f) >= probability)
        {
            return 0;
        }

        return rng.RandiRange(minAmount, maxAmount);
    }

}
