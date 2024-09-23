using Godot;
using System;

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

        FocusEntered += OnItemFocused;
        FocusExited += OnItemUnfocused;
    }

    public void SetSlotData(SlotData value)
    {
        _slotData = value;

        if (_slotData == null)
        {
            return;
        }

        textureRect.Texture = _slotData.item_data.ItemTexture;

        // hide the quanity label if there is no quantity
        if (_slotData.item_quantity <= 0)
        {
            label.Text = "";

        } else
        {
            label.Text = _slotData.item_quantity.ToString();
        }
    }


    private void OnItemUnfocused()
    {
        PauseMenu.Instance.UpdateItemDescription("");
        return;
    }

    private void OnItemFocused()
    {
        if(slotData != null)
        {
            if(slotData.item_data != null)
            {
                PauseMenu.Instance.UpdateItemDescription(slotData.item_data.ItemDescription);
            }
        }
        return;
    }
}
