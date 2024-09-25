using Godot;
using System;

/// <summary>
/// A base class for all states.  This is used to define the required
/// behavior for each state.
/// </summary>
public partial class PlayerAttackState : State
{
    PlayerController controllerOwner;

    // the animation player for our player controller
    AnimationPlayer animPlayer;  // for the movement animation
    AnimationPlayer attackAnimPlayer; // for the attack animation
    AudioStreamPlayer2D attackAudioStreamPlayer; // for the attack sound

    [Export] AudioStream attackSound;
    [Export(PropertyHint.Range, "1, 20, 0.5")] float decelerateSpeed = 5.0f;
    

    // states connected to this one
    private State walkState;
    private State deadState;
    private State idleState;

    private HurtBox hurtBox;

    private bool attacking = false;

    // Constructor
    public PlayerAttackState()
    {
    }

    public override void InitializeOwner()
    {
        // cast the stateOwner as a PlayerController so that we can access
        // its properties and methods
        this.controllerOwner = stateOwner as PlayerController;

        // add this setter here since it requires that controllerOwner has been set already
        animPlayer = controllerOwner.GetNode<AnimationPlayer>("AnimationPlayer");
        attackAnimPlayer = controllerOwner.GetNode<AnimationPlayer>("Sprite2D/AttackEffectSprite/AnimationPlayer");
        attackAudioStreamPlayer = controllerOwner.GetNode<AudioStreamPlayer2D>("Audio/AudioStreamPlayer2D");

        hurtBox = controllerOwner.GetNode<HurtBox>("Sprite2D/AttackHurtBox");
    }

    public override void _Ready()
    {
        walkState = GetNode<State>("../PlayerWalk");  // set the reference to the walk state node in the Godot tree
        deadState = GetNode<State>("../PlayerDead");  // set the reference to the walk state node in the Godot tree
        idleState = GetNode<State>("../PlayerIdle");  // set the reference to the walk state node in the Godot tree
    }

    // What happens when the player enters this State?
    public override async void EnterState()
    {
        InitializeOwner();

        this.controllerOwner.UpdateAnimation("attack");

        // ply the melee attackanimation
        GD.Print("-- attacking to " + controllerOwner.AnimDirection());
        attackAnimPlayer.Play("attack_" + controllerOwner.AnimDirection());

        // play the sound
        var rng = new RandomNumberGenerator();  // creat a random numbe generate
        attackAudioStreamPlayer.Stream = attackSound;
        attackAudioStreamPlayer.PitchScale = rng.RandfRange(0.9f, 1.1f); // tweak the pitch scale slightly
        attackAudioStreamPlayer.Play();

        // connect the signal so we can detect when the animation is finished
        // remember to disconnect in ExitState()
        animPlayer.AnimationFinished += EndAttack;

        attacking = true;

        // create a small delay when attacking
        await ToSignal(GetTree().CreateTimer(0.075f), SceneTreeTimer.SignalName.Timeout);

        if(attacking is true)
        {
            hurtBox.Monitoring = true;  // turn on the hurtbox monitoring
        }

        return;
    }

    // What happens when the player exits this State?
    public override void ExitState()
    {
        // disconnect from the signal so we don't get repeated warnings about already being connected
        animPlayer.AnimationFinished -= EndAttack;
        attacking = false;

        hurtBox.Monitoring = false; // turn it back off when we aren't attacking

        return;
    }

    // What happens during the _Process() update in this State?
    public override State Process(double delta)
    {
        GD.Print("player is attacking");
        controllerOwner.Velocity -= controllerOwner.Velocity * decelerateSpeed * (float)delta;

        if (controllerOwner.IsDead)
        {
            return deadState;
        }


        if(attacking == false)
        {
            if(controllerOwner.DirectionVector == Vector2.Zero)
            {
                return idleState;
            } else
            {
                return walkState;
            }
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

    private void EndAttack(StringName animName)
    {
        attacking = false;

        return;
    }
}
