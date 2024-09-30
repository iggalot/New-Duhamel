using Godot;

public partial class GlobalPlayerManager : Node
{
    public PackedScene PLAYER_SCENE { get; set; }
    public InventoryData INVENTORY_DATA { get; set; } = new InventoryData();

    [Signal] public delegate void InteractPressedEventHandler();

    // a function that can be called from outside of the GlobalPlayerManager to emit a signal for player ineraction pressed
    public void EmitInteractPressedSignal()
    {
        EmitSignal(SignalName.InteractPressed);
    }

    private static GlobalPlayerManager _instance;
    public static GlobalPlayerManager Instance => _instance;

    //public InventoryUI inventory { get; set; }

    public PlayerController player { get; set; }
    public PlayerHud playerHud { get; set; }

    public bool PlayerSpawned { get; set; } = false;

    public async override void _Ready()
    {
        // set our player controller scene so we can quickly reload when we transition to other areas
        PLAYER_SCENE = GD.Load<PackedScene>("res://player/player_controller.tscn");
        

        AddPlayerInstance();

        // set a small delay before we declare a player spawned, in case we are loading from another scene
        await ToSignal(GetTree().CreateTimer(0.2), SceneTreeTimer.SignalName.Timeout);
        PlayerSpawned = true;

        INVENTORY_DATA = GD.Load("res://scenes/ui/inventory/player_inventory.tres") as InventoryData;
    }

    public void AddPlayerInstance()
    {
        player = PLAYER_SCENE.Instantiate() as PlayerController;
        player.ZIndex = 2;
        player.YSortEnabled = true;
        AddChild(player);
        return;
    }

    public override void _EnterTree()
    {
        if(_instance != null)
        {
            this.QueueFree(); // The singleton is already loaded, kill this instance
        }
        _instance = this;
    }

    public void SetHealth(float hp, float max_hp)
    {
        player.MaxHitPoints = max_hp;
        player.HitPoints = hp;
        player.UpdateHitPoints(0); // force an update so that the HUD is updated -- passing a zero so nothing changes
        return;
    }

    public void SetPlayerPosition(Vector2 new_pos)
    {
        player.GlobalPosition = new_pos;
        return;
    }

    public void SetAsParent(Node2D p)
    {
        if (player.GetParent() != null)
        {
            player.GetParent().RemoveChild(player);
        }
        p.AddChild(player);
        return;
    }

    public void UnparentPlayer(Node2D p)
    {
        if (player.GetParent() != p)
        {
            return;
        }
        p.RemoveChild(player);
        return;
    }

    public void PlayAudio(AudioStream audio)
    {
        player.audio.Stream = audio;
        player.audio.Play();
    }
}
