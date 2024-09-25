using Godot;
using System;

public partial class TreasureChest : Node2D
{
    private ItemData _item_data;
    private int _quantity = 1;

    [Export] public ItemData itemData
    {
        get => _item_data;
        set {
            SetItemData(value);
        }
    }

    [Export]
    public int itemQuantity
    {
        get => _quantity;
        set
        {
            SetQuantity(value);
        }
    }

    bool isOpen = false;

    // getters and setters for scene nodes
    public Sprite2D sprite { get; set; }
    public Label label { get; set; }
    public AnimationPlayer animation_player { get; set; }
    public Area2D interact_area { get; set; }
    public PersistentDataHandler is_open_data { get; set; }

    public override void _Ready()
    {
        // set up our references to the nodes in the scene
        sprite = GetNode<Sprite2D>("ItemSprite");
        label = GetNode<Label>("ItemSprite/Label");
        animation_player = GetNode<AnimationPlayer>("AnimationPlayer");
        interact_area = GetNode<Area2D>("Area2D");
        is_open_data = GetNode<PersistentDataHandler>("PersistentDataIsOpen");

        UpdateTexture();
        UpdateLabel();

        if (Engine.IsEditorHint())
        {
            return;
        }

        interact_area.AreaEntered += OnAreaEnter;
        interact_area.AreaExited += OnAreaExit;
        is_open_data.DataLoaded += SetChestState;

        SetChestState();
    }

    private void SetChestState()
    {
        isOpen = is_open_data.Value;
        if(isOpen is true)
        {
            animation_player.Play("opened");
        } else
        {
            animation_player.Play("closed");
        }
    }

    private void OnAreaEnter(Area2D area)
    {
        // connect to the player interaction signal
        GlobalPlayerManager.Instance.InteractPressed += PlayerInteract;
        return;
    }

    private void OnAreaExit(Area2D area)
    {
        // disconnect from the player interaction signal
        GlobalPlayerManager.Instance.InteractPressed -= PlayerInteract;
        return;
    }

    private void PlayerInteract()
    {
        if(isOpen == true)
        {
            return;
        }
        isOpen = true;
        is_open_data.SetValue();

        animation_player.Play("open_chest");

        if(itemData != null && itemQuantity > 0)
        {
            GlobalPlayerManager.Instance.INVENTORY_DATA.AddItem(itemData, itemQuantity);
        } else
        {
            GD.PrintErr("No Items in Chest!");
            GD.PushError("No Items in Chest! Chest Name: " + Name);
        }
    }

    public void SetItemData(ItemData value)
    {
        _item_data = value;
        UpdateTexture();
    }

    private void SetQuantity(int value)
    {
        _quantity = value;
        UpdateLabel();
    }

    public void UpdateTexture()
    {
        if(itemData != null && sprite != null)
        {
            sprite.Texture = itemData.ItemTexture;
        }
    }

    public void UpdateLabel()
    {
        if(label != null)
        {
            if(itemQuantity <= 1)
            {
                label.Text = "";
            } else
            {
                label.Text = "x" + itemQuantity.ToString();
            }
        }
    }
}
