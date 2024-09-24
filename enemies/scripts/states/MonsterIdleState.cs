using Godot;
using System;

/// <summary>
/// A base class for all states.  This is used to define the required
/// behavior for each state.
/// </summary>
public partial class MonsterIdleState : State
{
    // who owns this specific state machine
    MonsterController controllerOwner;

    [Export] string animName = "idle";
    [Export] string spriteStatusName = "idle"; // name of the sprite animation to use while in this state


    [ExportCategory("AI")]
    [Export] private float stateDurationMin { get; set; } = 0.5f;
    [Export] private float stateDurationMax { get; set; } = 1.5f;
    [Export] private State nextState { get; set; }

    private float timer { get; set; } = 0.0f;

    private State wanderState;
    // Constructor
    public MonsterIdleState()
    {

    }

    public override void InitializeOwner()
    {
        // cast the stateOwner as a MonsterController so that we can access
        // its properties and methods
        this.controllerOwner = stateOwner as MonsterController;
    }

    public override void _Ready()
    {
        // set the after idle state to be a wander state
        wanderState = GetNode<State>("../MonsterWander");  // set the reference to the idle state node in the Godot tree
        nextState = wanderState;
    }

    public override void Init()
    {
        return;
    }

    // What happens when the player enters this State?
    public override void EnterState()
    {
        InitializeOwner();

        controllerOwner.Velocity = Vector2.Zero;
        var rng = new RandomNumberGenerator();
        timer = rng.RandfRange(stateDurationMin, stateDurationMax); // set a random timer duration

        // update animations and status symbols
        controllerOwner.UpdateAnimation(animName);
        controllerOwner.UpdateStatusSpriteAnimation(spriteStatusName);

        return;
    }

    // What happens when the player exits this State?
    public override void ExitState()
    {
        return;
    }

    // What happens during the _Process() update in this State?
    public override State Process(double delta)
    {
        // countdown our timer
        timer -= (float)delta;

        if(timer <= 0.0f)
        {
            return nextState;
        }
        //GD.Print("im idling");

        //// check if the monster is alerted --- if so, go to walk / search mode
        //if (controllerOwner.IsAlerted)
        //    return walkState;

        //// check the current velocity of the monster -- if its non-zero then the monster is walking
        //if (controllerOwner.DirectionUnitVector != Vector2.Zero)
        //    return walkState;

        //// otherwise, we are not moving, so set the velocity to zero.
        //controllerOwner.Velocity = Vector2.Zero;
        return null;
    }

    // What happens during the _PhysicsProcess() update in this State?
    public override State Physics(double delta)
    {
        return null;
    }

    // What happens with the input events in this State?
    public override State HandleInput(InputEvent input_event)
    {
        return null;
    }
}
