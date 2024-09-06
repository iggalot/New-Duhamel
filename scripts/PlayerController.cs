using Godot;
using System;

public partial class PlayerController : CharacterBody2D
{
    public const float Speed = 300.0f;

    private float friction = 0.25f;
    private float acceleration = 0.3f;

    private AnimatedSprite2D animatedSprite;
    private AnimationPlayer animationPlayer;
    private float animationTimer = 2.0f;
    private float animationTimerMax = 2.0f;

    private bool isDead = false;

    public override void _Ready()
    {
        animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public override void _Process(double delta)
    {
        if (isDead)
        {
            return;
        }
        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Velocity = processMovement((float)delta);

        MoveAndSlide();
    }

    private Vector2 processMovement(float delta)
    {
        Vector2 velocity = this.Velocity;
        Vector2 direction = new Vector2(0, 0);

        if (animationPlayer.IsPlaying())
        {
            animationTimer -= (float)delta;
            if (animationTimer <= 0.0f)
            {
                animationTimer = animationTimerMax;
                animationPlayer.Stop();
            }
            animatedSprite.Play("none");
            return velocity;
        }

        // reset the animated spite
        animatedSprite.Rotation = 0;

        if (Input.IsActionPressed("ui_left"))
        {
            direction += new Vector2(-1, 0);
            animatedSprite.Play("walk_left");
        }

        if (Input.IsActionPressed("ui_right"))
        {
            direction += new Vector2(1, 0);
            animatedSprite.Play("walk_right");
        }

        if (Input.IsActionPressed("ui_up"))
        {
            direction += new Vector2(0, -1);
            //animatedSprite.Play("walk_up");
            
            // stop our moving motion and then play the death animation
            isDead = true;
            animatedSprite.Play("none");
            animationPlayer.CurrentAnimation = "death";
            animationPlayer.Play();
        }

        if (Input.IsActionPressed("ui_down"))
        {
            direction += new Vector2(0, 1);
            animatedSprite.Play("walk_down");


        }

        Vector2 dir_unit_vec = direction.Normalized();

        if (dir_unit_vec != Vector2.Zero)
        {
            // if e are moving, then apply acceleration
            velocity.X = Mathf.Lerp(velocity.X, dir_unit_vec.X * Speed, acceleration);
            velocity.Y = Mathf.Lerp(velocity.Y, dir_unit_vec.Y * Speed, acceleration);
        }
        else
        {
            animatedSprite.Play("idle_front");
            // otherwise apply friction to slow us down
            velocity.X = Mathf.Lerp(velocity.X, 0, friction);
            velocity.Y = Mathf.Lerp(velocity.Y, 0, friction);
        }

        return velocity;

    }
}
