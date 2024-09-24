using Godot;

public partial class InventoryData : Resource
{
    private const int SLOT_COUNT = 16;

    [Export] public SlotData[] slots { get; set; } = new SlotData[SLOT_COUNT];

    public bool AddItem(ItemData item, int count = 1)
    {
        // does the item already exist in the inventory
        foreach (var s in slots)
        {
            if (s != null)
            {
                if (s.item_data == item)
                {
                    s.item_quantity += count;
                    return true;
                }
            }
        }

        // search for an empty slot -- if found, add the item to the slot at that index
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                SlotData new_slot = new SlotData();
                new_slot.item_data = item;
                new_slot.item_quantity = count;
                this.slots[i] = new_slot;
                return true;
            }
        }

        GD.Print("Inventory is full");  

        return false;
    }
}
