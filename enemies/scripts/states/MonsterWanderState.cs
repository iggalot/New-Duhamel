using Godot;
using System;

/// <summary>
/// A base class for all states.  This is used to define the required
/// behavior for each state.
/// </summary>
public partial class MonsterWanderState : State
{
    // who owns this specific state machine
    MonsterController controllerOwner;

    [Export] string animName = "walk"; // name of the animation to use while in this state
    [Export] string spriteStatusName = "wander"; // name of the sprite animation to use while in this state

    [Export] float wanderSpeed = 50.0f;

    [ExportCategory("AI")]
    [Export] private float stateAnimationDuration { get; set; } = 0.7f; // this needs to match duration of the animation clip
    
    // How many cycles this state should run for (based on the animation duration);
    [Export] private int stateCyclesMin { get; set; } = 1;
    [Export] private int stateCyclesMax { get; set; } = 3;
    [Export] private State nextState { get; set; }

    private float timer { get; set; } = 0.0f;
    private Vector2 direction { get; set; } = Vector2.Zero;

    private State idleState;
    // Constructor
    public MonsterWanderState()
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
    }

    public override void Init()
    {
        // must initialize the owner to get the casting correct.
        InitializeOwner();
        nextState = idleState;

        return;
    }

    // What happens when the player enters this State?
    public override void EnterState()
    {
        var rng = new RandomNumberGenerator();
        timer = rng.RandiRange(stateCyclesMin, stateCyclesMax) * stateAnimationDuration;

        var rand = rng.RandiRange(0, controllerOwner.DIR_4.Length - 1);
        direction = controllerOwner.DIR_4[rand];
        controllerOwner.Velocity = direction.Normalized() * wanderSpeed;
        controllerOwner.SetDirection(direction);

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
        timer -= (float)delta;

        if (timer < 0)
        {
            return nextState;
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
