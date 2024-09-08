using Godot;
using System;

public partial class PlayerController : CharacterBody2D
{
    public const float Speed = 300.0f;

    private float friction = 0.25f;
    private float acceleration = 0.3f;

    private AnimatedSprite2D animatedSprite;
    private AnimationPlayer animationPlayer;  // for a graphical animation of the character
    private AnimationPlayer playerMessageWindowAnimationPlayer; // for a graphical animation of the player message window
    private ColorRect playerMessageWindow;
    private float animationTimer = 2.0f;
    private float animationTimerMax = 2.0f;
    private float playerMessageAnimationTimer = 2.0f;
    private float playerMessageAnimationTimerMax = 2.0f;


    private bool isDead = false;
    private bool DeathAnimationHasPlayed = false;
    private bool DeathMessagePopupHasPlayed = false;
    private bool IsGameOver = false;

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("dead"))
        {
            // Set isDead to true to test death animation and gamve over cycle
            isDead = true;
        }
    }

    public override void _Ready()
    {
        animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        playerMessageWindowAnimationPlayer = GetNode<AnimationPlayer>("PlayerMessageWindow/AnimationPlayer");
        playerMessageWindow = GetNode<ColorRect>("PlayerMessageWindow");

        playerMessageWindow.Visible = false;
    }

    public override void _Process(double delta)
    {
        // is the game over?  do nothing more with the player
        if (IsGameOver)
        {
            return;
        }

        // decrease the timers
        if(playerMessageAnimationTimer > 0.0f && playerMessageAnimationTimer <= playerMessageAnimationTimerMax)
        {
            playerMessageAnimationTimer -= (float)delta;
        }
        // is the player dead?  Show the animation then end the game
        if (isDead)
        {
            // play the death animation for the character
            if (DeathAnimationHasPlayed is false)
            {
                if (!animationPlayer.IsPlaying() || playerMessageWindowAnimationPlayer.CurrentAnimation != "death")
                {
                    DoAnimationPlayer("death"); // send the death animation to the player -- "death" is defined in Godot's AnimationPlayer
                    DeathAnimationHasPlayed = true;
                }
            }

            // Display the death message popup
            if (DeathMessagePopupHasPlayed is false)
            {
                if (!playerMessageWindowAnimationPlayer.IsPlaying() || playerMessageWindowAnimationPlayer.CurrentAnimation != "message_window_popup")
                {
                    DoPlayerMessageWindowAnimation("U r ded!"); // display our death message here
                    DeathMessagePopupHasPlayed  = true;
                }
            }

            // are the animations finished? If so, signal the game has ended and dont let the palyer do anything else
            if(DeathMessagePopupHasPlayed && DeathAnimationHasPlayed && !animationPlayer.IsPlaying() && !playerMessageWindowAnimationPlayer.IsPlaying())
            {
                IsGameOver = true;
                return;
            }

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
            animatedSprite.Play("walk_up");

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

    private void DoAnimationPlayer(string anim_name)
    {
        if (animationPlayer.IsPlaying() is false && animationPlayer.CurrentAnimation != anim_name)
        {
            animationPlayer.CurrentAnimation = anim_name;
            animationPlayer.Play();
            animationTimer = animationTimerMax;
        }

    }

    private void DoPlayerMessageWindowAnimation(string message)
    {
        if(playerMessageWindowAnimationPlayer.IsPlaying() is false && playerMessageWindowAnimationPlayer.CurrentAnimation != "message_window_popup")
        {
            playerMessageWindow.Visible = true;
            Label label = playerMessageWindow.GetNode<Label>("Label");
            label.Text = message;
            playerMessageWindowAnimationPlayer.Play("message_window_popup");
            playerMessageAnimationTimer = playerMessageAnimationTimerMax;
        }


    }

}
