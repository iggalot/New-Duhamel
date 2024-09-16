using Godot;

/// <summary>
/// A base class for all states.  This is used to define the required
/// behavior for each state.
/// </summary>
public partial class PlayerIdleState : State
{
    PlayerController controllerOwner;

    private State walkState;
    private State deadState;
    private State attackState;


    // Constructor
    public PlayerIdleState()
    {

    }

    public override void InitializeOwner()
    {
        // cast the stateOwner as a PlayerController so that we can access
        // its properties and methods
        this.controllerOwner = stateOwner as PlayerController;
    }

    public override void _Ready()
    {
        walkState = GetNode<State>("../PlayerWalk");  // set the reference to the walk state node in the Godot tree
        deadState = GetNode<State>("../PlayerDead");  // set the reference to the walk state node in the Godot tree
        deadState = GetNode<State>("../PlayerAttack");  // set the reference to the walk state node in the Godot tree

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
        GD.Print("player is idle");
        if (controllerOwner.IsDead)
        {
            return deadState;
        }
        if(controllerOwner.DirectionVector != Vector2.Zero)
        {
            return walkState;
        }

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

        // if idle and we attack, move to attack state
        if (input_event.IsActionPressed("attack"))
        {
            GD.Print("attacking from idle state");

            return attackState;
        }

        return null;
    }
}
