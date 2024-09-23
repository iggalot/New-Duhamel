using Godot;

public partial class InventoryData : Resource
{
    private const int SLOT_COUNT = 16;

    [Export] SlotData[] slots = new SlotData[SLOT_COUNT];

    public void AddItem()
    {

    }
}
