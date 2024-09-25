using Godot;
using System;

/// <summary>
/// A base class for all states.  This is used to define the required
/// behavior for each state.
/// </summary>
public partial class MonsterDeadState : State
{
    // the stateOwner from the State class cast into the appropriate controller type -- 
    // in this case to distinguish between player states and monster states
    private MonsterController controllerOwner;
    private PlayerController player;

    private State idleState;

    //[Export] public float WalkSpeed = 100.0f;

    // Constructor
    public MonsterDeadState()
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
        idleState = GetNode<State>("../MonsterIdle");  // set the reference to the idle state node in the Godot tree

        // get the player character from the scnee tree
        player = GlobalPlayerManager.Instance.player;

    }

    // What happens when the player enters this State?
    public override void EnterState()
    {
        controllerOwner.UpdateAnimation("dead");
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
        //GD.Print("I'm dead");

        if(controllerOwner.IsAlerted is false)
        {
            return idleState;
        } else
        {
            // get the vector between the monster and the player and compute the unit vector
            // and then set the velocity.
            Vector2 direction = Vector2.Zero;  // get the direction to the player.
            controllerOwner.DirectionUnitVector = Vector2.Zero;
            controllerOwner.Velocity = Vector2.Zero;

            // update the animation if the monster changes direction.

            UpdateVelocityAndSpeed(0);
            controllerOwner.UpdateAnimation("dead");

            return this;
        }
    }

    private void UpdateVelocityAndSpeed(float speed)
    {
        // get the vector between the monster and the player and compute the unit vector
        // and then set the velocity.
        Vector2 direction = controllerOwner.GlobalPosition.DirectionTo(player.GlobalPosition);  // get the direction to the player.
        controllerOwner.DirectionUnitVector = direction.Normalized(); ;
        controllerOwner.Velocity = controllerOwner.DirectionUnitVector * speed;
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
