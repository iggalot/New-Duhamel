using Godot;
using System;

/// <summary>
/// A base class for all states.  This is used to define the required
/// behavior for each state.
/// </summary>
public partial class MonsterDestroyState : State
{
    // who owns this specific state machine
    MonsterController controllerOwner;

    [Export] string animName = "destroy"; // name of the animation to use while in this state
    [Export] string spriteStatusName = "dead"; // name of the sprite animation to use while in this state

    [Export] float knockbackSpeed = 300.0f;
    [Export] float decelerateSpeed = 50.0f;

    [ExportCategory("AI")]

    public Vector2 damagePosition { get; set; }
    private Vector2 direction { get; set; } = Vector2.Zero;

    // Constructor
    public MonsterDestroyState()
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
    }

    public override void Init()
    {
        // must initialize the owner to get the casting correct.
        InitializeOwner();

        // connect to the enemy damage signal in monster controller
        controllerOwner.EnemyKilled += OnEnemyKilled;

        return;
    }

    // What happens when the player enters this State?
    public override void EnterState()
    {
        controllerOwner.IsInvulernable = true;
        direction = controllerOwner.GlobalPosition.DirectionTo(damagePosition);
        controllerOwner.SetDirection(direction);
        controllerOwner.Velocity = direction.Normalized() * -knockbackSpeed;

        // update animations and status symbols
        controllerOwner.UpdateAnimation(animName);
        controllerOwner.UpdateStatusSpriteAnimation(spriteStatusName);
        controllerOwner.animationPlayer.AnimationFinished += OnAnimationFinished;
        DisableHurtBox();
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
        controllerOwner.Velocity -= controllerOwner.Velocity * decelerateSpeed * (float)delta;

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

    private void OnEnemyKilled(HurtBox hurt_box)
    {
        damagePosition = hurt_box.GlobalPosition;
        controllerOwner.stateMachine.ChangeState(this);
    }

    private void OnAnimationFinished(StringName animName)
    {
        controllerOwner.QueueFree();
    }

    private void DisableHurtBox()
    {
        HurtBox hurt_box = controllerOwner.GetNodeOrNull<HurtBox>("HurtBox");
        if(hurt_box != null)
        {
            hurt_box.Monitoring = false;
        }
    }
}
