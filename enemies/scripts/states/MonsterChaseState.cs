using Godot;
using System;

/// <summary>
/// A base class for all states.  This is used to define the required
/// behavior for each state.
/// </summary>
public partial class MonsterChaseState : State
{
    const string pathfinder_scene_path = "res://enemies/scripts/abilities/pathfinding/pathfinding.tscn";
    public PackedScene PATHFINDER = GD.Load<PackedScene>(pathfinder_scene_path);

    // the stateOwner from the State class cast into the appropriate controller type -- 
    // in this case to distinguish between player states and monster states
    // who owns this specific state machine

    private MonsterController controllerOwner;
    private PlayerController player;

    private Pathfinding pathfinder;

    [Export] string animName = "chase"; // name of the animation to use while in this state
    [Export] string spriteStatusName = "chase"; // name of the sprite animation to use while in this state

    [Export] float chaseSpeed = 50.0f; // how fast the monster moves towards the player when chasing
    [Export] float turnRate = 0.5f;  // how quickly the monster turns to face the player

    [ExportCategory("AI")]
    [Export] public VisionArea visionArea { get; set; }
    [Export] public HurtBox attackArea { get; set; }
    [Export] private float stateAggroDuration { get; set; } = 0.5f;

    [Export] private float stateAnimationDuration { get; set; } = 0.7f; // this needs to match duration of the animation clip

    // How many cycles this state should run for (based on the animation duration);
    [Export] private State nextState { get; set; }

    private float timer { get; set; } = 0.0f;
    private Vector2 direction { get; set; } = Vector2.Zero;

    private State idleState;
    private State stunState;
    private State destroyState;

    bool canSeePlayer = false;

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
        stunState = GetNode<State>("../MonsterStun");
        destroyState = GetNode<State>("../MonsterDestroy");

        visionArea = GetNode<VisionArea>("../../VisionArea");
        attackArea = GetNode<HurtBox>("../../Sprite2D/AttackHurtBox");
    }

    public override void Init()
    {
        // must initialize the owner to get the casting correct.
        InitializeOwner();

 //       nextState = idleState;

        return;
    }

    // What happens when the player enters this State?
    public override void EnterState()
    {
        pathfinder = PATHFINDER.Instantiate() as Pathfinding;
        controllerOwner.AddChild(pathfinder);


        //GD.Print("monster is chasing -- before linking signals");
        if (visionArea != null)
        {
            //GD.Print("-- signals linked");

            visionArea.PlayerEntered += OnPlayerEnter;
            visionArea.PlayerExited += OnPlayerExit;
        }
        //GD.Print("monster is chasing -- after linking signals");


        var rng = new RandomNumberGenerator();
        timer = stateAggroDuration;

        // update animations and status symbols
        controllerOwner.UpdateAnimation(animName);
        controllerOwner.UpdateStatusSpriteAnimation(spriteStatusName);

        canSeePlayer = true;

        // turn on the monitoring for the attack area
        if(attackArea != null)
        {
            attackArea.Monitoring = true;
        }

        return;
    }

    // What happens when the player exits this State?
    public override void ExitState()
    {
        pathfinder.QueueFree();

        //visionArea.canSeePlayer = false;
        ////GD.Print("exiting chase state");

        //if (visionArea != null)
        //{
        //    //GD.Print("-- signals unlinked");

        //    visionArea.PlayerEntered -= OnPlayerEnter;
        //    visionArea.PlayerExited -= OnPlayerExit;
        //}
        if(attackArea != null)
        {
            attackArea.Monitoring = false;
        }

        canSeePlayer = false;

        return;
    }

    // What happens during the _Process() update in this State?
    public override State Process(double delta)
    {
        //Vector2 new_dir = controllerOwner.GlobalPosition.DirectionTo(GlobalPlayerManager.Instance.player.GlobalPosition);
        //float x = Mathf.Lerp(direction.X, new_dir.X, turnRate);
        //float y = Mathf.Lerp(direction.Y, new_dir.Y, turnRate);
        //direction = new Vector2(x, y);
        float direction_x = Mathf.Lerp(direction.X, pathfinder.move_dir.X, turnRate);
        float direction_y = Mathf.Lerp(direction.Y, pathfinder.move_dir.Y, turnRate);
        direction = new Vector2(direction_x, direction_y);

        controllerOwner.Velocity = direction * chaseSpeed;

        if (controllerOwner.SetDirection(direction) is true)
        {
            controllerOwner.UpdateAnimation(animName);
            controllerOwner.UpdateStatusSpriteAnimation(spriteStatusName);
        }

        if(canSeePlayer == false)
        {
            timer -= (float)delta;

            if (timer < 0)
            {
                return nextState;
            }
        } else
        {
            timer = stateAggroDuration;
        }
        //if (visionArea != null)
        //{
        //    if (visionArea.canSeePlayer == true)
        //    {
        //        timer = stateAggroDuration; // reset the aggro timer
        //        return this;
        //    }
        //}



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

    private void OnPlayerEnter(Area2D area)
    {
        canSeePlayer = true;
        ////GD.Print("in chase state -- player entered");
        //visionArea.canSeePlayer = true;

        if((stateMachine.currentState == stunState) || (stateMachine.currentState == destroyState))
        {
            return;
        }

        stateMachine.ChangeState(this);
        return;
    }

    private void OnPlayerExit(Area2D area)
    {
        canSeePlayer = false;
        //GD.Print("in chase state -- player entered");

        //visionArea.canSeePlayer = false;
        return;
    }


}
