using Godot;

public partial class InventoryData : Resource
{
    private const int SLOT_COUNT = 16;

    [Export] public SlotData[] slots { get; set; } = new SlotData[SLOT_COUNT];


    public InventoryData()
    {
        //// need to return a duplicate of the slots_array since the resources seem to be internconnected and will duplicate at 
        //// all instances of the same resouce when using C#
        //SlotData[] new_slots = new SlotData[SLOT_COUNT];
        //for (int i = 0; i < slots.Length; i++)
        //{
        //    SlotData new_slot = new SlotData();
        //    new_slot.item_quantity = 0;
        //    new_slot.item_data = null;
        //    if (slots[i] != null)
        //    {
        //        if (slots[i].item_data != null)
        //        {
        //            new_slot.item_data = slots[i].item_data;
        //            new_slot.item_quantity = slots[i].item_quantity;
        //        }
        //    }

        //    new_slots[i] = new_slot;
        //}

        //slots = new_slots;

    }
    public void AddItem()
    {

    }
}
