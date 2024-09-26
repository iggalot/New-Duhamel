using Godot;
using System;

/// <summary>
/// A base class for all states.  This is used to define the required
/// behavior for each state.
/// </summary>
public partial class MonsterIdleState : State
{
    // who owns this specific state machine
    MonsterController controllerOwner;

    [Export] string animName = "idle";
    [Export] string spriteStatusName = "idle"; // name of the sprite animation to use while in this state

    [Export] public VisionArea visionArea { get; set; }
    [Export] public HurtBox attackArea { get; set; }


    [ExportCategory("AI")]
    [Export] private float stateDurationMin { get; set; } = 0.5f;
    [Export] private float stateDurationMax { get; set; } = 1.5f;
    [Export] private State nextState { get; set; }

    private float timer { get; set; } = 0.0f;


    private State wanderState;
    private State stunState;
    private State chaseState;

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
        // set the after idle state to be a wander state
        wanderState = GetNode<State>("../MonsterWander");  // set the reference to the idle state node in the Godot tree
        stunState = GetNode<State>("../MonsterStun");  // set the reference to the idle state node in the Godot tree
        chaseState = GetNode<State>("../MonsterChase");  // set the reference to the idle state node in the Godot tree

        nextState = wanderState;

        visionArea = GetNode<VisionArea>("../../VisionArea");
        attackArea = GetNode<HurtBox>("../../Sprite2D/AttackHurtBox");
    }

    public override void Init()
    {
        return;
    }

    // What happens when the player enters this State?
    public override void EnterState()
    {
        GD.Print("monster is idling");
        InitializeOwner();

        if (visionArea != null)
        {
            visionArea.PlayerEntered += OnPlayerEnter;
            visionArea.PlayerExited += OnPlayerExit;
        }

        controllerOwner.Velocity = Vector2.Zero;
        var rng = new RandomNumberGenerator();
        timer = rng.RandfRange(stateDurationMin, stateDurationMax); // set a random timer duration

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
        // countdown our timer
        timer -= (float)delta;

        if (visionArea != null)
        {
            if (visionArea.canSeePlayer == true)
            {
                return chaseState;
            }
        }

        if (timer <= 0.0f)
        {
            return nextState;
        } 
        
        return this;


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

    private void OnPlayerEnter(Area2D area)
    {
        GD.Print("in idle state -- player entered vision area");
        if (visionArea != null)
        {
            visionArea.canSeePlayer = true;
        }

        if (stateMachine.currentState == stunState)
        {
            return;
        }

        nextState = chaseState;  // change to chase state if player has entered the vision cone
//        stateMachine.ChangeState(chaseState);
        return;
    }

    private void OnPlayerExit(Area2D area)
    {
        if (visionArea != null)
        {
            visionArea.canSeePlayer = false;
        }

        GD.Print("in idle state -- player entered");

        return;
    }
}
