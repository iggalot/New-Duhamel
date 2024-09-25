using Godot;
using System;

public partial class PlayerCamera : Camera2D
{
    public override void _Ready()
    {
        GlobalLevelManager.Instance.TileMapBoundsChanged += UpdateLimits;
        UpdateLimits(GlobalLevelManager.Instance.currentTileMapBounds);
        return;
    }

    public void UpdateLimits(Vector2[] bounds)
    {
        if(bounds == null) 
        { 
            return; 
        }

        LimitLeft = (int)(bounds[0].X);
        LimitTop = (int)(bounds[0].Y);
        LimitRight = (int)(bounds[1].X);
        LimitBottom = (int)(bounds[1].Y);
    }
}
