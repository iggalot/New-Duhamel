using Godot;
using System;
using System.Text.RegularExpressions;
using static StateMachine;

public partial class MonsterController : CharacterBody2D
{
	public const float Speed = 100.0f;
	public const float JumpVelocity = -400.0f;

	[Export] public bool ShouldFlee { get; set; } = false;
    [Export] public bool ShouldChasePlayer { get; set; } = false;
	[Export] public bool ShouldSleep { get; set; } = false;
	[Export] public bool ShouldAttack { get; set; } = false;


    public override void _PhysicsProcess(double delta)
	{
		// find the player controller in the scene tree
		PlayerController playerController = GetTree().Root.GetNode<PlayerController>("GameManager/PlayerController");
        if (playerController != null)
        {
			Vector2 playerPosition = playerController.GlobalPosition;
			processMonsterAction((float)delta, playerPosition);
        } else
		{
			processMonsterAction((float)delta, null);
		}

		Vector2 velocity = Velocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		processMovement((float)delta);

		Velocity = velocity;
		MoveAndSlide();
	}

	public void processMonsterAction(float delta, Vector2? player_pos)
	{
		// if the player is dead let's sleep
		if(player_pos == null)
		{
			ShouldFlee = false;
            ShouldChasePlayer = false;
            ShouldSleep = true;
            ShouldAttack = false;
			return;
        }
        else
        {
			var glob_pos = this.GlobalPosition;
			var distance = Math.Sqrt(Math.Pow(glob_pos.X - player_pos.Value.X, 2) + Math.Pow(glob_pos.Y - player_pos.Value.Y, 2));
			GD.Print("dist to player: " + distance);
			// if it's too close, lets turn away
			if(distance < 100)
			{
                ShouldFlee = true;
                ShouldChasePlayer = false;
                ShouldSleep = false;
                ShouldAttack = false;
            } 
			// lets attack
			else if (distance < 200)
			{
                ShouldFlee = false;
                ShouldChasePlayer = false;
                ShouldSleep = false;
                ShouldAttack = true;
			}
			// let's chase the player
            else if (distance < 300)
			{
				{
                    ShouldFlee = false;
                    ShouldChasePlayer = true;
                    ShouldSleep = false;
                    ShouldAttack = false;
				}

            }
			// otherwise nothing to do so lets sleep
			else
			{
                ShouldFlee = false;
                ShouldChasePlayer = false;
                ShouldSleep = true;
                ShouldAttack = false;
			}

        }
	}

    public override void _Ready()
    {

    }

    public void processMovement(float delta)
	{

	}

	public void Flee()
	{
        PlayerController playerController = GetTree().Root.GetNode<PlayerController>("GameManager/PlayerController");
        Vector2 direction = playerController.GlobalPosition - this.GlobalPosition;

		//reverse the direction
        Vector2 unit_direction = -direction.Normalized();
        Velocity = unit_direction * Speed;
        GD.Print("Monster is fleeing");
	}

	public void ChasePlayer()
	{
        PlayerController playerController = GetTree().Root.GetNode<PlayerController>("GameManager/PlayerController");
		Vector2 direction = playerController.GlobalPosition - this.GlobalPosition;
		Vector2 unit_direction = direction.Normalized();
        Velocity = unit_direction * Speed;

        GD.Print("I'm chasing the player");
	}

	public void Sleep()
	{
		Velocity = new Vector2(0, 0);
        GD.Print("I'm sleeping");
	}

	public void Attack()
	{
        PlayerController playerController = GetTree().Root.GetNode<PlayerController>("GameManager/PlayerController");
        Vector2 direction = playerController.GlobalPosition - this.GlobalPosition;
        Vector2 unit_direction = direction.Normalized();
        GD.Print("I'm attacking");
	}
}
