using Godot;
using System;

/// <summary>
/// A base class for all states.  This is used to define the required
/// behavior for each state.
/// </summary>
public partial class PlayerWalkState : State
{
    // the stateOwner from the State class cast into the appropriate controller type -- 
    // in this case to distinguish between player states and monster states
    private PlayerController controllerOwner;

    [Export] public float walk_speed { get; set; } = 0.0f;

    // references to the connected states of this state
    private State idleState;
    private State deadState;
    private State attackState;



    //[Export] public float WalkSpeed = 100.0f;

    // Constructor
    public PlayerWalkState()
    {
    }

    public override void InitializeOwner()
    {
        // cast the stateOwner as a MonsterController so that we can access
        // its properties and methods
        controllerOwner = stateOwner as PlayerController;
        walk_speed = controllerOwner.WalkSpeed;
        
        
    }

    public override void _Ready()
    {
        idleState = GetNode<State>("../PlayerIdle");  // set the reference to the idle state node in the Godot tree
        deadState = GetNode<State>("../PlayerDead");  // set the reference to the idle state node in the Godot tree
        attackState = GetNode<State>("../PlayerAttack");  // set the reference to the idle state node in the Godot tree

        // get the player character from the scnee tree
        controllerOwner = GetTree().Root.GetNode<PlayerController>("PlayerManager/PlayerController");
    }

    // What happens when the player enters this State?
    public override void EnterState()
    {
        this.controllerOwner.UpdateAnimation("walk");
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
        //GD.Print("player is walking");

        var speed = controllerOwner.WalkSpeed;

        if (controllerOwner.IsDead is true)
        {
            return deadState;
        } else if (controllerOwner.DirectionVector == Vector2.Zero)
        {
            return idleState;
        } else
        {
            controllerOwner.Velocity = controllerOwner.DirectionVector.Normalized() * walk_speed;

            if(controllerOwner.SetDirection() is true)
            {
                controllerOwner.UpdateAnimation("walk");
            }
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
        // if idle and we attack, move to attack state
        if (input_event.IsActionPressed("attack"))
        {
            GD.Print("attacking from walking state");

            return attackState;
        }

        if (input_event.IsActionPressed("interact"))
        {
            // emit our global player interaction signal
            GlobalPlayerManager.Instance.EmitInteractPressedSignal();
        }

        return null;
    }
}
