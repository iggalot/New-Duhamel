using Godot;
using System;

/// <summary>
/// A base class for all states.  This is used to define the required
/// behavior for each state.
/// </summary>
public partial class PlayerDeadState : State
{
    PlayerController controllerOwner;

    private State walkState;
    private State idleState;

    // Constructor
    public PlayerDeadState()
    {

    }

    public override void InitializeOwner()
    {
        // cast the stateOwner as a MonsterController so that we can access
        // its properties and methods
        this.controllerOwner = stateOwner as PlayerController;
    }

    public override void _Ready()
    {
        walkState = GetNode<State>("../PlayerWalk");  // set the reference to the walk state node in the Godot tree
        idleState = GetNode<State>("../PlayerIdle");  // set the reference to the walk state node in the Godot tree

    }

    // What happens when the player enters this State?
    public override void EnterState()
    {
        InitializeOwner();

        this.controllerOwner.UpdateAnimation("dead");
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
        GD.Print("player is dead");
        controllerOwner.Velocity = Vector2.Zero;
        controllerOwner.DirectionVector = Vector2.Zero;

        // set our state to dead
        if (controllerOwner.IsDead)
        {
            return this;
        }

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
