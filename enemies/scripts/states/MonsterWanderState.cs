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

    [Export] public VisionArea visionArea { get; set; }
    [Export] public HurtBox attackArea { get; set; }


    [ExportCategory("AI")]
    [Export] private float stateAnimationDuration { get; set; } = 0.7f; // this needs to match duration of the animation clip
    
    // How many cycles this state should run for (based on the animation duration);
    [Export] private int stateCyclesMin { get; set; } = 1;
    [Export] private int stateCyclesMax { get; set; } = 3;
    [Export] private State nextState { get; set; }

    private float timer { get; set; } = 0.0f;
    private Vector2 direction { get; set; } = Vector2.Zero;

    private State idleState;
    private State wanderState;
    private State stunState;
    private State chaseState;
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
        wanderState = GetNode<State>("../MonsterWander");  // set the reference to the idle state node in the Godot tree
        stunState = GetNode<State>("../MonsterStun");  // set the reference to the idle state node in the Godot tree
        chaseState = GetNode<State>("../MonsterChase");  // set the reference to the idle state node in the Godot tree

        visionArea = GetNode<VisionArea>("../../VisionArea");
        attackArea = GetNode<HurtBox>("../../Sprite2D/AttackHurtBox");
    }

    public override void Init()
    {
        // must initialize the owner to get the casting correct.
        InitializeOwner();

//        nextState = idleState;

        return;
    }

    // What happens when the player enters this State?
    public override void EnterState()
    {
        //GD.Print("monster is wandering -- before linking signals");
        if (visionArea != null)
        {
            //GD.Print("-- signals linked");

            visionArea.PlayerEntered += OnPlayerEnter;
            visionArea.PlayerExited += OnPlayerExit;
        }
        //GD.Print("monster is wandering -- after linking signals");



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
        //GD.Print("exiting wander state");

        if (visionArea != null)
        {
            //GD.Print("-- signals unlinked");

            visionArea.PlayerEntered -= OnPlayerEnter;
            visionArea.PlayerExited -= OnPlayerExit;
        }
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
        //GD.Print("in wander state -- player entered vision area");

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
        //GD.Print("in wander state -- player entered");

        return;
    }
}
