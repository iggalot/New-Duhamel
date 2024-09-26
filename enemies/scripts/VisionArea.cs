using Godot;
using System;

public partial class VisionArea : Area2D
{
    [Signal] public delegate void PlayerEnteredEventHandler(Area2D area);
    [Signal] public delegate void PlayerExitedEventHandler(Area2D area);

    public bool canSeePlayer = false;

    public float timer { get; set; } = 1.0f;
    public bool searching { get; set; } = false;
    public float searchingTimer { get; set; } = 3.0f;
    public float searchingTimerMax { get; set; } = 3.0f;

    public float sweepStartAngle = 0;
    public float sweepEndAngle = 180;
    public float sweepStepSize = 5.0f;


    public Vector2 searchDirection { get; set; } = new Vector2(0, -1);

    public override void _Ready()
    {
        BodyEntered += OnBodyEnter;
        BodyExited += OnBodyExit;

        var p = GetParent();
        if(p is MonsterController)
        {
            ((MonsterController)p).DirectionChanged += OnDirectionChanged;
        }
    }

    public override void _Process(double delta)
    {
        timer -= (float)delta;

        // are we in vision searching mode -- i.e. was the play detected and now is not?
        if(searching == true)
        {
            searchingTimer -= (float)delta;
            float[] temp_vec = new float[] { searchDirection.X, searchDirection.Y };
            Vector2 sweepVector = RotateVector(temp_vec, sweepStepSize );

            Rotation += sweepVector.Angle() * (float)delta;
        }

        // check if the search timer has expired -- if it has stop searching and reset the timer to max
        if(searchingTimer <= 0)
        {
            searching = false;
            searchingTimer = searchingTimerMax; // reset the timer for the next time

            // set the searching direction to the direction the enemy was moving in of the enemy
            var p = GetParent();
            if (p is MonsterController)
            {
                MonsterController mc = (MonsterController)p;
                mc.SetDirection(mc.DirectionVector);   // this will reset the cone
            }
        }
    }

    private void OnBodyEnter(Node2D body)
    {
        // if the body enters the vision area, we can see the direction and now their location
        searchDirection = GlobalPosition.DirectionTo(body.GlobalPosition).Normalized();

        // turn off searching since the body's location is known
        searching = false;

        // signal that the player can be seen
        canSeePlayer = true;
        
        GD.Print(body.Name + " has entered the interaction area");

        if(body is PlayerController)
        {
//            GD.Print("--" + body.Name + " is emitting signal in OnBodyEnter");

            EmitSignal(SignalName.PlayerEntered);
        }

        return;
    }

    private void OnBodyExit(Node2D body)
    {
        // signal that the player can no longer be seen
        canSeePlayer = false;

        // start searching sweep again
        StartSearchingSweep(searchDirection);

//        GD.Print(body.Name + " has exited the interaction area");
        if (body is PlayerController)
        {
 //           GD.Print("--" + body.Name + "is emitting signal in OnBodyExit");
            EmitSignal(SignalName.PlayerExited);
        }

        return;
    }

    /// <summary>
    /// signal that the vision area should start sweeping to find the player again.
    /// This function somewhat arbitrarily rotates the vision area until it collides with the player.
    /// </summary>
    /// <param name="searchDirection"></param>
    private void StartSearchingSweep(Vector2 searchDirection)
    {
        searching = true;
    }

    // change the direction of the vision area to the new_direction
    private void OnDirectionChanged(Vector2 new_direction)
    {
        // The direction here are based on the assumption that the vision area is oriented downwards
        // -- +y direction -- which is the 0 degree direction -- positive angles assumed clockwise.

        if(new_direction == Vector2.Down)
        {
            RotationDegrees = 0;
        } else if (new_direction == Vector2.Up) 
        {
            RotationDegrees = 180;
        } else if (new_direction == Vector2.Left)
        {
            RotationDegrees = 90;
        } else if (new_direction == Vector2.Right)
        {
            RotationDegrees = 270;
        } else
        {
            RotationDegrees = 0;
        }

        //GD.Print("Rotate of vision area to: " + RotationDegrees);
    }

    private static Vector2 RotateVector(float[] vector, float angle)
    {
        float angleRadians = angle * (float)Math.PI / 180.0f;

        // rotation matrix for 2D
        float cosTheta = (float)Math.Cos(angleRadians);
        float sinTheta = (float)Math.Sin(angleRadians);

        // Apply the 2D rotation matrix
        float[] rotatedVector = new float[2];
        rotatedVector[0] = vector[0] * cosTheta - vector[1] * sinTheta;
        rotatedVector[1] = vector[0] * sinTheta + vector[1] * cosTheta;

        return new Vector2(rotatedVector[0], rotatedVector[1]);
    }

    private static Vector2 SweepVector(Vector2 vector, float startAngle, float endAngle, float stepSize)
    {
        Vector2 new_vector = vector;
        for (float angle = startAngle; angle <= endAngle; angle += stepSize)
        {
            new_vector = RotateVector(new float[]{vector.X, vector.Y}, angle);
        }

        return new_vector;


    }
}
