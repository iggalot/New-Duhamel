using Godot;
using System;

/// <summary>
/// A base class for all states.  This is used to define the required
/// behavior for each state.
/// </summary>
public partial class PlayerStunState : State
{
    // the stateOwner from the State class cast into the appropriate controller type -- 
    // in this case to distinguish between player states and monster states
    private PlayerController controllerOwner;

    [Export] public float knockbackSpeed = 1000.0f;
    [Export] public float decelerateSpeed = 10.0f;
    [Export] public float invulnerableDuration = 1.0f;

    HurtBox hurtBox;
    Vector2 direction;

    State nextState = null;

    // references to the connected states of this state
    private State idleState;

    //[Export] public float WalkSpeed = 100.0f;

    // Constructor
    public PlayerStunState()
    {
    }

    public override void InitializeOwner()
    {
        // cast the stateOwner as a MonsterController so that we can access
        // its properties and methods
        controllerOwner = stateOwner as PlayerController;
        
        
    }

    public override void _Ready()
    {
        idleState = GetNode<State>("../PlayerIdle");  // set the reference to the idle state node in the Godot tree

        // get the player character from the scnee tree
        controllerOwner = GetTree().Root.GetNode<PlayerController>("GameManager/PlayerController");
    }

    public override void Init()
    {
        controllerOwner.PlayerDamaged += OnPlayerDamaged;
    }

    // What happens when the player enters this State?
    public override void EnterState()
    {
        // connect the animation signal
        controllerOwner.animationPlayer.AnimationFinished += OnAnimationFinished;

        direction = controllerOwner.GlobalPosition.DirectionTo(hurtBox.GlobalPosition);
        controllerOwner.Velocity = direction * -knockbackSpeed;
        controllerOwner.SetDirection();
        controllerOwner.UpdateAnimation("stun");
        controllerOwner.MakeInvulnerable(invulnerableDuration);
        controllerOwner.effectAnimationPlayer.Play("damaged");
        return;
    }

    // What happens when the player exits this State?
    public override void ExitState()
    {
        nextState = null;
        // disconnect the signal
        controllerOwner.animationPlayer.AnimationFinished -= OnAnimationFinished;

        return;
    }

    // What happens during the _Process() update in this State?
    public override State Process(double delta)
    {
        GD.Print("player is stunned -- " + (decelerateSpeed * delta).ToString() );
        controllerOwner.Velocity -= controllerOwner.Velocity * (float)(decelerateSpeed * delta );
        return nextState;
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

    private void OnPlayerDamaged(HurtBox hurt_box)
    {
        hurtBox = hurt_box;
        stateMachine.ChangeState(this);
    }

    private void OnAnimationFinished(StringName animName)
    {
        nextState = idleState;
    }
}
