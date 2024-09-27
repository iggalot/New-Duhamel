using Godot;
using Godot.Collections;
using System.Linq;

public partial class InventoryData : Resource
{
    private const int SLOT_COUNT = 16;

    [Export] public SlotData[] slots { get; set; } = new SlotData[SLOT_COUNT];

    public InventoryData()
    {
        ConnectSlots();
        return;
    }


    private void ConnectSlots()
    {
        foreach (var s in slots)
        {
            if (s != null)
            {
                s.Changed += OnSlotChanged;
            }
        }
    }

    private void OnSlotChanged()
    {
        bool matchFound = false;
        foreach (var s in slots)
        {
            if (s != null)
            {
                if (s.item_quantity < 1)
                {
                    s.Changed -= OnSlotChanged;

                    for (int i = 0; i < slots.Length; i++)
                    {
                        if (slots[i] == s)
                        {
                            slots[i] = null;
                            EmitChanged();
                            break;
                        }
                    }

                    if (matchFound is true)
                    {
                        break;
                    }
                }
            }
        }
        return;
    }

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
                new_slot.Changed += OnSlotChanged;
                return true;
            }
        }

        GD.Print("Inventory is full");

        return false;
    }

    // gather the inventory into an array or dictionary
    public Dictionary<string, Dictionary<string, int>> GetSaveData()
    {
        Dictionary<string, Dictionary<string, int>> item_save = new Dictionary<string, Dictionary<string, int>>();
        for (int i = 0; i < slots.Length; i++)
        {
            if ((slots[i] != null))
            {
                item_save.Add("item"+i.ToString(), ItemToSave(slots[i]));
            }
        }
        return item_save;
    }

    // convert each inventory item into a dictionary
    public Dictionary<string, int> ItemToSave(SlotData slot)
    {
        Dictionary<string, int> result = new Dictionary<string, int>();

        if(slot != null)
        {

            var quantity = slot.item_quantity;
            string resource_path = "";
            if(slot.item_data != null)
            {
                resource_path = slot.item_data.ResourcePath;
            }
            result.Add(resource_path, quantity);
        }
        return result;
    }

    public void ParseSaveData(Dictionary<string, Dictionary<string, int>> save_data)
    {
        // get our number of items
        //int array_size = save_data.Count;

        // clear all of the slots data
        for(int i = 0; i < slots.Length; i++)
        {
            slots[i] = null;
        }

        //SlotData[] new_slot_arr = new SlotData[array_size];
        
        // now update the items in the inventory
        for (int i = 0; i < slots.Length; i++)
        {
            string dict_key = "item"+i.ToString();  // look for our unique item identifiers
            if (save_data.ContainsKey(dict_key))
            {
                slots[i] = ItemFromSave(save_data[dict_key]);
            }
        }

        ConnectSlots();

        return;
    }

    public SlotData ItemFromSave(Dictionary<string, int> save_obj)
    {
        if(save_obj == null)
        {
            return null;
        }

        // create the new inventory slot
        SlotData new_slot = new SlotData();
        
        var dict_keys = save_obj.Keys.ToArray();
        var filepath = dict_keys[0];

        new_slot.item_data = GD.Load<ItemData>(filepath);
        new_slot.item_quantity = (int)save_obj[filepath];

        return new_slot;
    }

    public bool UseItem( ItemData item, int count = 1)
    {
        foreach (SlotData s in slots)
        {
            if (s != null)
            {
                if(s.item_data == item && s.item_quantity >= count)
                {
                    s.item_quantity -= count;
                    return true;
                }
            }
        }

        return false;
    }
}
