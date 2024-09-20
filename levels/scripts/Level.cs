using Godot;
using System;

public partial class Level : Node2D
{
    public override void _Ready()
    {
        this.YSortEnabled = true;
        GlobalPlayerManager.Instance.SetAsParent(this);
        
        // setup the camera limits based on the tilemap boundary -- this is a merged hack of the original TileMapLayer script
        // -- we had to combine that script into this one since GODOT only allows one script file per object
        GlobalLevelManager.Instance.bounds = GetTileMapBounds();
        GlobalLevelManager.Instance.ChangeTileMapBounds(GlobalLevelManager.Instance.bounds);
        
        return;
    }

    /// <summary>
    /// this is the parent of the scene that contains the tilemap layers.
    /// </summary>
    /// <param name="area_node"></param>
    /// <returns></returns>
    private Vector2[] GetTileMapBounds()
    {
        float max_x = -100000;  // set the max limit to a very small number
        float max_y = -100000;  // set the max limit to a very small number
        float min_x = 100000;   // set the min limit to a very large number
        float min_y = 100000;   // set the min limit to a very large number

        // scan our children for tilemap layers...
        foreach (Node node in GetChildren())
        {
            if (node is TileMapLayer)
            {
                TileMapLayer layer = node as TileMapLayer;
                float x1 = layer.GetUsedRect().Position.X * layer.RenderingQuadrantSize;
                float y1 = layer.GetUsedRect().Position.Y * layer.RenderingQuadrantSize;
                float x2 = layer.GetUsedRect().End.X * layer.RenderingQuadrantSize;
                float y2 = layer.GetUsedRect().End.Y * layer.RenderingQuadrantSize;

                Console.WriteLine("layer bounds: " + x1 + " " + y1 + " " + x2 + " " + y2);

                // left x
                if (x1 < min_x)
                {
                    min_x = x1;
                }
                // left y
                if (y1 < min_y)
                {
                    min_y = y1;
                }

                // right x
                if (x2 > max_x)
                {
                    max_x = x2;
                }

                // right y
                if (y2 > max_y)
                {
                    max_y = y2;
                }
            }
        }

        Vector2[] bounds = new Vector2[2];
        bounds[0].X = min_x;  // top left x
        bounds[0].Y = min_y;    // top_lefy y
        bounds[1].X = max_x;  // bottom right x
        bounds[1].Y = max_y;    // bottom right y

        return bounds;
    }
}
