using Godot;
using System;

public partial class GlobalLevelManager : Node
{
    private static GlobalLevelManager _instance;
    public static GlobalLevelManager Instance => _instance;

    [Signal] public delegate void TileMapBoundsChangedEventHandler(Vector2[] bounds);

    public Vector2[] currentTileMapBounds { get; set; }


    public override void _EnterTree()
    {
        if (_instance != null)
        {
            this.QueueFree(); // The singleton is already loaded, kill this instance
        }
        _instance = this;
    }

    public void ChangeTileMapBounds(Vector2[] bounds)
    {
        currentTileMapBounds = bounds;
        EmitSignal(SignalName.TileMapBoundsChanged, bounds);
    }
}
