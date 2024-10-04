using Godot;
using System;

public partial class InventoryUI : Control
{
    PackedScene INVENTORY_SLOT { get; set;}

    [Export] public InventoryData data { get; set; }

    public int focusIndex { get; set; } = 0;

    public override void _Ready()
    {
        // set up our getters and setters for the GODOT tree nodes
        INVENTORY_SLOT = GD.Load<PackedScene>("res://scenes/ui/inventory/inventory_slot.tscn");


        // subscribe to events in the pause menu
        PauseMenu.Instance.Shown += UpdateInventory;
        PauseMenu.Instance.Hidden += ClearInventory;

        // clear the inventory to begin with
        ClearInventory();

        data.Changed += OnInventoryChanged;
    }

    private void OnInventoryChanged()
    {
        var i = focusIndex;

        ClearInventory();
        UpdateInventory();
    }

    public void ClearInventory()
    {
        foreach (Node c in GetChildren())
        {
            c.QueueFree();
        }
    }

    public async void UpdateInventory()
    {
        // update the data here to match the global inventory data
        data = GlobalPlayerManager.Instance.INVENTORY_DATA;
//        OnInventoryChanged();

        foreach (var s in data.slots)

            //foreach (var s in data.slots)
        {
            Node new_slot = INVENTORY_SLOT.Instantiate();
            AddChild(new_slot);
            (((InventorySlotUI)new_slot).slotData) = s;
            ((Control)new_slot).FocusEntered += OnItemFocused;
        }

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); // wait a frame so that the inventory is updated before we grab focus


        // set the focus to the appropriate element
        ((Control)GetChild(focusIndex)).GrabFocus();
    }

    private void OnItemFocused()
    {
        for (int i = 0; i < GetChildCount(); i++)
        {
            if (((Control)GetChild(i)).HasFocus())
            {
                focusIndex = i;
                return;
            }
        }

        return;
    }

    public override void _UnhandledInput(InputEvent this_event)
    {
        GD.Print("trying to use inventory item");
        // an action for the gamepad controller to use an inventory item
        if (this_event.IsActionPressed("use_inventory_item"))
        {
            GD.Print("-- using inventory item");
            if (PauseMenu.Instance.IsPaused == true)
            {
                GD.Print("-- Used inventory item");
                GetTree().Paused = false;  // unpause the game so we can use the item
                // use the item
                ((InventorySlotUI)GetChild(focusIndex)).UseItem();
                GetTree().Paused = true;  // repause the game after we use the item
            }

            GetViewport().SetInputAsHandled();
        }
    }
}
