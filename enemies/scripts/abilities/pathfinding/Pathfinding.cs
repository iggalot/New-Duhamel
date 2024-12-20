using Godot;
using System;
using System.Linq;

public partial class Pathfinding : Node2D
{
    Vector2[] vectors = new Vector2[8] {
        new Vector2(0, -1),     // Up
        new Vector2(1, -1),     // Up and Right
        new Vector2(1, 0),      // Right
        new Vector2(1, 1),      // Down and Right
        new Vector2(0, 1),      // Down
        new Vector2(-1, 1),     // Down and Left
        new Vector2(-1, 0),     // Left
        new Vector2(-1, -1)     // Up and Left
    };
    

    public float[] obstacles = new float[8];
    public float[] interests = new float[8];
    public float[] outcomes = new float[8];

    public RayCast2D[] rays = new RayCast2D[8];

    public Vector2 move_dir { get; set; } = Vector2.Zero;
    public Vector2 best_path { get; set; } = Vector2.Zero;

    // References
    Timer timer { get; set; }


    public override void _Ready()
    {
        // set our references
        timer = GetNode<Timer>("Timer");

        // Gather all raycast2Dnodes
        int current_count = 0;
        foreach (var c in GetChildren())
        {
            if(c is RayCast2D)
            {
                rays[current_count] = c as RayCast2D;
                current_count++;
            }
        }

        // Normalize all vectors
        for(int i = 0; i < vectors.Length; i++)
        {
            vectors[i] = vectors[i].Normalized();
        }

        // Perform initial pathfinder function
        SetPath();

        // Connect our timer
        timer.Timeout += () => { SetPath(); };
    }

    public override void _Process(double delta)
    {
    }

    // Set the "best_path" vector by checking for desired direction and considering obstacles
    private void SetPath()
    {
        // Get direction to the player
        Vector2 player_dir = GlobalPosition.DirectionTo(GlobalPlayerManager.Instance.player.GlobalPosition);

        // Reset obstacles values to 0
        for(int i = 0; i < obstacles.Length; i++)
        {
            obstacles[i] = 0;
            outcomes[i] = 0;
            interests[i] = 0;
        }

        // Check each Raycast2D for collisions and update valus in obstacles array
        for(int i = 0; i < rays.Length; i++)
        {
            if(rays[i].IsColliding())
            {
                obstacles[i] += 4;
                obstacles[(i + rays.Length + 1) % rays.Length] += 1;
                obstacles[(i + rays.Length - 1) % rays.Length] += 1;
            }
        }

        // If there are no obstacles, recommend path in direction of player
        if(obstacles.Max() == 0)
        {
            best_path = player_dir;
            return;
        }

        // Populate our interest array.  This array contains alues that represent
        // the desireability of each direction
        for (int i = 0; i < vectors.Length; i++)
        {
            // we want the dot propduct to measure the overlap between the two vectors.  Higher values means more similar vectors
            interests[i] = vectors[i].Dot(player_dir);
        }

        // Populate outcomes array, by combininge interest and obstacle arrays
        for (int i = 0; i < vectors.Length; i++)
        {
            outcomes[i] = interests[i] - obstacles[i];
        }



        //GD.Print("Interests: " + interests[0] + ", " + interests[1] + ", " + interests[2] + ", " + interests[3] + ", " + interests[4] + ", " + interests[5] + ", " + interests[6] + ", " + interests[7]);
        //GD.Print("Obstacles: ", obstacles[0], ", ", obstacles[1], ", ", obstacles[2], ", ", obstacles[3], ", ", obstacles[4], ", ", obstacles[5], ", ", obstacles[6], ", ", obstacles[7]);
        //GD.Print("Outcomes: ", outcomes[0], ", ", outcomes[1], ", ", outcomes[2], ", ", outcomes[3], ", ", outcomes[4], ", ", outcomes[5], ", ", outcomes[6], ", ", outcomes[7]);


        // Set the best_path with the Vector2 that corresponds with the outcome with the highest value
        best_path = vectors[Array.IndexOf(outcomes, outcomes.Max())];
    }
}
