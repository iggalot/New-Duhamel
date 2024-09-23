using Godot;

public partial class SlotData : Resource
{
    [Export] public ItemData item_data { get; set; } =  new ItemData();
    [Export] public int item_quantity { get; set; } = 0;
}
