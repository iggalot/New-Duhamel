using Godot;
using System;

/// <summary>
/// A base class for all states.  This is used to define the required
/// behavior for each state.
/// </summary>
public partial class MonsterChaseState : State
{
    // the stateOwner from the State class cast into the appropriate controller type -- 
    // in this case to distinguish between player states and monster states
    private MonsterController controllerOwner;
    private PlayerController player;

    // references to the connected states of this state
    private State idleState;
    private State walkState;
    private State attackState;
    private State deadState;

    //[Export] public float WalkSpeed = 100.0f;

    // Constructor
    public MonsterChaseState()
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
        walkState = GetNode<State>("../MonsterWalk");  // set the reference to the idle state node in the Godot tree
        attackState = GetNode<State>("../MonsterAttack");  // set the reference to the idle state node in the Godot tree
        deadState = GetNode<State>("../MonsterDead");  // set the reference to the idle state node in the Godot tree

        // get the player character from the scnee tree
        player = GetTree().Root.GetNode<PlayerController>("GameManager/PlayerController");
    }

    // What happens when the player enters this State?
    public override void EnterState()
    {
        controllerOwner.UpdateAnimation("chase");
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
        GD.Print("I'm chasing");

        // if our owner is gone or dead, then delete the item from the tree.
        if (controllerOwner == null)
        {
            QueueFree();
        }

        if (player == null || controllerOwner.IsAlerted == false)
        {
            // player has vanished so stop moving and return to idle state
            controllerOwner.Velocity = Vector2.Zero;
            controllerOwner.DirectionUnitVector = Vector2.Zero;
            return walkState;
        }

        var speed = controllerOwner.WalkSpeed;
        State new_state = null;
        string animation = "chase";

        if (controllerOwner.IsDead is true)
        {
            return deadState;
        }

        if (controllerOwner.IsAlerted is false)
        {
            return idleState;
        }
        else
        {
            var distance = controllerOwner.GlobalPosition.DistanceTo(player.GlobalPosition);

            if (distance < controllerOwner.MIN_ATTACK_DISTANCE)
            {
                new_state = attackState;
                speed = controllerOwner.AttackSpeed;
                animation = "attack";

            }
            else if (distance < controllerOwner.MIN_CHASE_DISTANCE)
            {
                speed = controllerOwner.ChaseSpeed;
                new_state = this;
                animation = "chase";
            }
            else if (distance < controllerOwner.MIN_SEARCH_DISTANCE)
            {
                speed = controllerOwner.SearchSpeed;
                new_state = walkState;
                animation = "search";
            }
            else if (distance < controllerOwner.MAX_DEFAULT_DISTANCE)
            {
                speed = 0;
                new_state = idleState;
                animation = "idle";
            }
            else
            {
                return null;
            }

            UpdateVelocityAndSpeed(speed);
            controllerOwner.UpdateAnimation(animation);
            return new_state;
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
