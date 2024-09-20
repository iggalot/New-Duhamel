using Godot;
using System;

public partial class GlobalLevelManager : Node
{
    private static GlobalLevelManager _instance;
    public static GlobalLevelManager Instance => _instance;

    [Signal] public delegate void TileMapBoundsChangedEventHandler(Vector2[] bounds);

    public Vector2[] currentTileMapBounds { get; set; }

    // The tilemap bounds extents as a VEctor2[2] for upper left and lower right corner of bounding box
    public Vector2[] bounds { get; set; }

    public override void _Ready()
    {
        //bounds = GetTileMapBounds();
        //ChangeTileMapBounds(GetTileMapBounds());
    }



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
