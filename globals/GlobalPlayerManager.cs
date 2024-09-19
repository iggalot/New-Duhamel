using Godot;

public partial class GlobalPlayerManager : Node
{
    private static GlobalPlayerManager _instance;
    public static GlobalPlayerManager Instance => _instance;

    public PlayerController player { get; set; }
    public PlayerHud playerHud { get; set; }

    public override void _EnterTree()
    {
        if(_instance != null)
        {
            this.QueueFree(); // The singleton is already loaded, kill this instance
        }
        _instance = this;
    }
}
