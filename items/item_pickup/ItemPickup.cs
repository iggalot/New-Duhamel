using Godot;
using System;

[Tool]
public partial class ItemPickup : Node2D
{
    private ItemData _itemData = new ItemData();

    [Export]
    public ItemData itemData
    {
        get => _itemData;
        set
        {
            //_itemData = value;
            //UpdateTexture();
            SetItemData(value);
        }
    }

    // getters and setters for the node
    Area2D area_2d { get; set; }
    Sprite2D sprite_2d { get; set; }
    AudioStreamPlayer2D audio_stream_player_2d { get; set; }

    public override void _Ready()
    {
        // set up the getters and setters for the tree nodes of this scene
        area_2d = GetNode<Area2D>("Area2D");
        sprite_2d = GetNode<Sprite2D>("Sprite2D");
        audio_stream_player_2d = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");

        UpdateTexture();

        if (Engine.IsEditorHint())
        {
            return;
        }

        area_2d.BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        GD.Print(body.Name + " has entered the interaction area");
        if (body is PlayerController)
        {
            if(itemData != null)
            {
                if (GlobalPlayerManager.Instance.INVENTORY_DATA.AddItem(itemData) is true)
                {
                    ItemPickedUp();
                }
            }
        }
    }

    public async void ItemPickedUp()
    {
        // now that the item has been picked up, we can disconnect from the signal
        area_2d.BodyEntered -= OnBodyEntered;
        audio_stream_player_2d.Play();
        Visible = false;
        await ToSignal(audio_stream_player_2d, "finished");
        QueueFree();

    }

    public void SetItemData(ItemData value)
    {
        _itemData = value;
        UpdateTexture();
        return;
    }

    public void UpdateTexture()
    {
        if((itemData != null) && (sprite_2d != null))
        {
            sprite_2d.Texture = itemData.ItemTexture;
        }

        return;
    }
}
