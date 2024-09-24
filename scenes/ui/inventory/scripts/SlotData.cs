using Godot;
using System;

public partial class SlotData : Resource
{
    private int _qty;

    [Export] public ItemData item_data { get; set; } =  new ItemData();
    [Export]
    public int item_quantity
    {
        get => _qty;
        set
        {
            SetQuantity(value);
        }
    }

    private void SetQuantity(int qty)
    {
        _qty = qty;
        if(_qty < 1)
        {
            EmitChanged();
        }
    }
}
