using Godot;
using System;

/// <summary>
/// A base class for all states.  This is used to define the required
/// behavior for each state.
/// </summary>
public partial class MonsterIdleState : State
{
    MonsterController controllerOwner;

    private State walkState;
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
        walkState = GetNode<State>("../MonsterWalk");  // set the reference to the idle state node in the Godot tree
    }

    // What happens when the player enters this State?
    public override void EnterState()
    {
        InitializeOwner();

        this.controllerOwner.UpdateAnimation("idle");
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
        GD.Print("im idling");

        // check if the monster is alerted --- if so, go to walk / search mode
        if (controllerOwner.IsAlerted)
            return walkState;

        // check the current velocity of the monster -- if its non-zero then the monster is walking
        if (controllerOwner.DirectionUnitVector != Vector2.Zero)
            return walkState;

        // otherwise, we are not moving, so set the velocity to zero.
        controllerOwner.Velocity = Vector2.Zero;
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
