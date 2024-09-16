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
    private PlayerController player;

    // references to the connected states of this state
    private State idleState;
    private State deadState;


    //[Export] public float WalkSpeed = 100.0f;

    // Constructor
    public PlayerWalkState()
    {
    }

    public override void InitializeOwner()
    {
        // cast the stateOwner as a MonsterController so that we can access
        // its properties and methods
        player = stateOwner as PlayerController;
        
    }

    public override void _Ready()
    {
        idleState = GetNode<State>("../PlayerIdle");  // set the reference to the idle state node in the Godot tree
        deadState = GetNode<State>("../PlayerDead");  // set the reference to the idle state node in the Godot tree

        // get the player character from the scnee tree
        player = GetTree().Root.GetNode<PlayerController>("GameManager/PlayerController");
    }

    // What happens when the player enters this State?
    public override void EnterState()
    {
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
        GD.Print("player is walking");

        var speed = player.WalkSpeed;
        State new_state = null;
        string animation = "walk_down";
        string animated_sprite_string = "walk_down";

        if (player.IsDead is true)
        {
            speed = 0.0f;
            new_state = deadState;
            animated_sprite_string = "dead";
            animation = "dead";
        } else if (player.DirectionUnitVector == Vector2.Zero)
        {
            speed = 0.0f;
            new_state = idleState;
            animated_sprite_string = "idle_front";
        } else
        {
            speed = player.WalkSpeed;
            new_state = this;
            animated_sprite_string = "walk_down";
        }

        UpdateVelocityAndSpeed(speed);

        //if (player.SetDirection()){
        //    player.UpdateAnimation("walk_down");
        //}

        if(animation != String.Empty)
        {
            player.UpdateAnimation(animation);
        }

        player.UpdatePlayerAnimatedSprite();


        return new_state;
    }

    private void UpdateVelocityAndSpeed(float speed)
    {
        // get the vector between the monster and the player and compute the unit vector
        // and then set the velocity.
        Vector2 direction = player.DirectionUnitVector;  // get the direction to the player.
        player.DirectionUnitVector = direction.Normalized(); ;
        player.Velocity = player.DirectionUnitVector * speed;
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
