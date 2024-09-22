using Godot;
using System;

public partial class GlobalLevelManager : Node
{
    private static GlobalLevelManager _instance;
    public static GlobalLevelManager Instance => _instance;

    [Signal] public delegate void LevelLoadStartedEventHandler();
    [Signal] public delegate void LevelLoadedEventHandler();
    [Signal] public delegate void TileMapBoundsChangedEventHandler(Vector2[] bounds);

    public Vector2[] currentTileMapBounds { get; set; }
    public string targetTransition { get; set; }
    public Vector2 positionOffset { get; set; }

    // The tilemap bounds extents as a VEctor2[2] for upper left and lower right corner of bounding box
    public Vector2[] bounds { get; set; }

    public async override void _Ready()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        EmitSignal(SignalName.LevelLoaded);
    }

    public async void LoadNewLevel(
        string level_path,
        string target_transition,
        Vector2 position_offset)
    {
        GetTree().Paused = true;

        targetTransition = target_transition;
        positionOffset = position_offset;

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); // level transition is not instant

        EmitSignal(SignalName.LevelLoadStarted);

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        GetTree().ChangeSceneToFile(level_path);

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); // level transition is not instant

        GetTree().Paused = false;

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        EmitSignal(SignalName.LevelLoaded);

        return;
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
