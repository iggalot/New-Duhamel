using Godot;
using System;

[Tool]
public partial class LevelTransition : Area2D
{
    private int _size = 2;
    private SIDE _side = SIDE.LEFT;
    private bool _snapToGrid = false;

    enum SIDE {  LEFT, RIGHT, TOP, BOTTOM }

    [Export(PropertyHint.File, "*.tscn")] string level { get; set; }
    [Export] string targetTransitionArea { get; set; } = "LevelTransition";

    [ExportCategory("Collision Area Settings")]
    [Export(PropertyHint.Range, "1, 12, 1, or_greater")]
    int size
    {
        get => _size;
        set
        {
            _size = value;
            UpdateArea();
        }
    }

    [Export] SIDE side
    {
        get => _side;
        set
        {
            _side = value;
            UpdateArea();
        }
    }
    [Export]
    bool snapToGrid
    {
        get => _snapToGrid;
        set
        {
            _snapToGrid = value;
            SnapToGrid();
        }

    }// node getters and setters
        CollisionShape2D collisionShape { get; set; }

    public override async void _Ready()
    {
        // set the node getters and setters
        collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

        UpdateArea();

        if (Engine.IsEditorHint())
        {
            return;
        }

        Monitoring = false;
        
        PlacePlayer();

        await ToSignal(GlobalLevelManager.Instance, "LevelLoaded");

        Monitoring = true;

        BodyEntered += OnPlayerEntered;

        return;
    }

    public void OnPlayerEntered(Node2D p)
    {
        GlobalLevelManager.Instance.LoadNewLevel(level, targetTransitionArea, GetOffset());
        return;
    }

    public void PlacePlayer()
    {
        if(this.Name != GlobalLevelManager.Instance.targetTransition)
        {
            return;
        }

        GlobalPlayerManager.Instance.SetPlayerPosition(GlobalPosition + GlobalLevelManager.Instance.positionOffset);
    }

    public Vector2 GetOffset()
    {
        Vector2 offset = Vector2.Zero;
        Vector2 player_position = GlobalPlayerManager.Instance.player.GlobalPosition;
        
        if(side == SIDE.LEFT || side == SIDE.RIGHT)
        {
            offset.Y = player_position.Y - GlobalPosition.Y;
            offset.X = 16;
            if (side == SIDE.LEFT)
            {
                offset.X *= -1;
            }
        }
        else
        {
            offset.X = player_position.X - GlobalPosition.X;
            offset.Y = 16;
            if (side == SIDE.TOP)
            {
                offset.Y *= -1;
            }
        }


        return offset;
    }

    public void UpdateArea()
    {
        Vector2 new_rect = new Vector2(16, 16);
        Vector2 new_position = Vector2.Zero;

        if(side == SIDE.TOP)
        {
            new_rect.X *= size;
            new_position.Y -= 16;
        } else if (side == SIDE.BOTTOM)
        {
            new_rect.X *= size;
            new_position.Y += 16;
        } else if (side == SIDE.LEFT)
        {
            new_rect.Y *= size;
            new_position.X -= 16;
        } else if (side == SIDE.RIGHT)
        {
            new_rect.Y *= size;
            new_position.X += 16;
        }

        if(collisionShape == null)
        {
            collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        }

        // cast to rectangular since this shape is a rectangle in our scene
        ((RectangleShape2D)collisionShape.Shape).Size = new_rect;
        collisionShape.Position = new_position;
    }

    public void SnapToGrid()
    {
        float x = (float)Math.Round(Position.X / 16) * 16;
        float y = (float)Math.Round(Position.Y / 16) * 16;

        Position = new Vector2(x, y);
    }
}
