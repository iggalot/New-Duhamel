using Godot;

public partial class GlobalPlayerManager : Node
{
    public PackedScene PLAYER_SCENE { get; set; }
    private static GlobalPlayerManager _instance;
    public static GlobalPlayerManager Instance => _instance;

    public PlayerController player { get; set; }
    public PlayerHud playerHud { get; set; }

    public bool PlayerSpawned { get; set; } = false;

    public async override void _Ready()
    {
        // set our player controller scene so we can quickly reload when we transition to other areas
        PLAYER_SCENE = GD.Load<PackedScene>("res://scenes/player_controller.tscn");

        AddPlayerInstance();

        // set a small delay before we declare a player spawned, in case we are loading from another scene
        await ToSignal(GetTree().CreateTimer(0.2), SceneTreeTimer.SignalName.Timeout);
        PlayerSpawned = true;
    }

    public void AddPlayerInstance()
    {
        player = PLAYER_SCENE.Instantiate() as PlayerController;
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
        p.RemoveChild(player);
        return;
    }
}
