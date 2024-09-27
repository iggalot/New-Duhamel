using Godot;
using System;

public partial class ItemDropper : Node2D
{
    private ItemData _itemData;
    public PackedScene PICKUP { get; set; }

    bool hasDropped { get; set; } = false;

    public Sprite2D sprite { get; set; }
    public PersistentDataHandler hasDroppedData { get; set; }
    public AudioStreamPlayer audio { get; set; }

    [Export] public ItemData itemData
    {
        get => _itemData;
        set
        {
            SetItemData(value);
        }
    }


    public override void _Ready()
    {
        PICKUP = GD.Load<PackedScene>("res://items/item_pickup/item_pickup.tscn") as PackedScene;

        sprite = GetNode<Sprite2D>("Sprite2D");
        hasDroppedData = GetNode<PersistentDataHandler>("PersistentDataHandler");
        audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");

        if(Engine.IsEditorHint() is true)
        {
            UpdateTexture();
            return;
        }

        sprite.Visible = false;

        hasDroppedData.DataLoaded += OnDataLoaded;
        OnDataLoaded();
    }

    public void DropItem()
    {
        if(hasDropped is true)
        {
            return;
        }

        hasDropped = true;

        ItemPickup drop = PICKUP.Instantiate() as ItemPickup;
        drop.itemData = itemData;
        AddChild(drop);
        drop.PickedUp += OnDropPickedUp;
        audio.Play();

    }

    private void OnDropPickedUp()
    {
        hasDroppedData.SetValue();   // saves the persistent data
    }

    private void OnDataLoaded()
    {
        hasDropped = hasDroppedData.Value;
    }

    private void SetItemData(ItemData value)
    {
        _itemData = value;
        UpdateTexture();
        return;
    }

    private void UpdateTexture()
    {
        if(Engine.IsEditorHint() is true)
        {
            if(itemData != null && (sprite != null))
            {
                sprite.Texture = itemData.ItemTexture;
            }
        }
    }
}
