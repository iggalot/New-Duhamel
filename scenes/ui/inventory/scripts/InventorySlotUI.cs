using Godot;

public partial class InventorySlotUI : Button
{
    private SlotData _slotData;
    public SlotData slotData
    {
        get { return _slotData; }
        set
        {
            SetSlotData(value);
        }
    }

    TextureRect textureRect { get; set; }
    Label label { get; set; }

    public override void _Ready()
    {
        // getters and setters for the nodes of this scene element
        textureRect = GetNode<TextureRect>("TextureRect");
        label = GetNode<Label>("Label");

        textureRect.Texture = null;
        label.Text = "";
    }

    public void SetSlotData(SlotData value)
    {
        _slotData = value;

        if (_slotData == null)
        {
            return;
        }

        textureRect.Texture = _slotData.item_data.ItemTexture;
        label.Text = _slotData.item_quantity.ToString();
    }


}
