using Godot;

public partial class InventoryUI : Control
{
    PackedScene INVENTORY_SLOT { get; set;}

    [Export] public InventoryData data { get; set; }

    public override void _Ready()
    {
        // set up our getters and setters for the GODOT tree nodes
        INVENTORY_SLOT = GD.Load<PackedScene>("res://scenes/ui/inventory/inventory_slot.tscn");


        // subscribe to events in the pause menu
        PauseMenu.Instance.Shown += UdateInventory;
        PauseMenu.Instance.Hidden += ClearInventory;

        // clear the inventory to begin with
        ClearInventory();
    }

    public void ClearInventory()
    {
        foreach (Node c in GetChildren())
        {
            c.QueueFree();
        }
    }

    public void UdateInventory()
    {
        foreach (var s in data.slots)
        {
            Node new_slot = INVENTORY_SLOT.Instantiate();
            AddChild(new_slot);
            (((InventorySlotUI)new_slot).slotData) = s;
        }

        // Set the focus to the first item in the list of children
        ((Control)GetChild(0)).GrabFocus();
    }
}
