using Godot;
using System;
using System.Collections.Generic;

public partial class Inventory : Control
{
    private const int DEFAULT_INVENTORY_SIZE = 16;

    public ItemController[] Contents { get; set; } = new ItemController[DEFAULT_INVENTORY_SIZE];
    public int Capacity { get; set; } = DEFAULT_INVENTORY_SIZE;


    private GridContainer gridContainer;
    private PackedScene inventorySlotSCene;
    private string inventorySlotPath = "res://scenes/ui/inventory/inventory_slot.tscn";

    public override void _Ready()
    {
        gridContainer = GetNode<GridContainer>("ColorRect/MarginContainer/GridContainer");
        inventorySlotSCene = GD.Load<PackedScene>(inventorySlotPath);
        populateButtons();

        //// initialize our inventory with null items to start with
        //for (int i = 0; i < Capacity; i++)
        //{
        //    Contents.Add(null);
        //}
    }

    // populate the buttons with our inventory items
    private void populateButtons()
    {
        for (int i = 0; i < Capacity; i++)
        {
            InventorySlot slot = inventorySlotSCene.Instantiate<InventorySlot>() as InventorySlot;
            slot.Item = Contents[i];
            slot.Name = slot.Name + i.ToString();
            //slot.Position = new Vector2((i % 4) * 40, (i % 4) * 40);
            gridContainer.AddChild(slot);
        }
    }

    public void ShowInventory()
    {
        //GD.Print("this grid container has " + this.gridContainer.GetChildCount());

        //// empty the existing grid container
        //foreach(Node node in container.GetChildren())
        //{
        //    node.QueueFree();
        //}
    }
}
