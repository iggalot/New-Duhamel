using Godot;
using System;

/// <summary>
/// A base class for all states.  This is used to define the required
/// behavior for each state.
/// </summary>
public partial class State : Node
{
    public CharacterBody2D stateOwner { get; set; } = null;

    public override void _Ready()
    {
        return;
    }

    // overridden by the indiviual states to set the properly casted owner (to distinguish between monsters and players)
    public virtual void InitializeOwner()
    {
        return;
    }

    // What happens when the player enters this State?
    public virtual void EnterState()
    {
        return;
    }

    // What happens when the player exits this State?
    public virtual void ExitState()
    {
        return;
    }

    // What happens during the _Process() update in this State?
    public virtual State Process(double delta)
    {
        return null;
    }

    // What happens during the _PhysicsProcess() update in this State?
    public virtual State Physics(double delta)
    {
        return null;
    }

    // What happens with the input events in this State?
    public virtual State HandleInput(InputEvent input_event)
    {
        return null;
    }
}
