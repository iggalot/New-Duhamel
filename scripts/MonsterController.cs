using Godot;
using System;

public partial class MonsterController : CharacterBody2D
{
    PackedScene monsterScene;
    private static string monsterScenePath = "res://scenes/monster_controller.tscn";

    private const float MAX_CHASE_TIMER_DRATION = 5.0f;
    private const float MAX_SEARCH_TIMER_DURATION = 10.0f;


    private const float MIN_ATTACK_DISTANCE = 50.0f;
    private const float MIN_CHASE_DISTANCE = 125.0f;
    private const float MIN_SEARCH_DISTANCE = 225.0f;

    private const float MAX_DEFAULT_DISTANCE = 100000.0f;


    public float Speed { get; set; } = 50.0f;

    [Export] public float HitPoints { get; set; } = 100;
    public float MaxHitPoints { get; set; } = 100;

    private float ChaseTimer { get; set; } = MAX_CHASE_TIMER_DRATION;
    private float ChaseTimerMax { get; set; } = MAX_CHASE_TIMER_DRATION;
    private float AttackTimer { get; set; } = MAX_SEARCH_TIMER_DURATION;
    private float AttackTimerMax { get; set; } = MAX_SEARCH_TIMER_DURATION;


    private float AIDecisionTimer { get; set; } = 5.0f;
    private float AIDecisionTimerMax { get; set; } = 5.0f;


    [Export] public bool IsAlerted { get; set; } = false;
    [Export] public bool IsInAttackRange { get; set; } = false;
    [Export] public bool IsInChaseRange { get; set; } = false;
    [Export] public bool IsInSearchRange { get; set; } = false;
    [Export] public bool IsDead { get; set; } = false;



    [Export] public bool ShouldFlee { get; set; } = false;
    [Export] public bool ShouldChasePlayer { get; set; } = false;
	[Export] public bool ShouldSleep { get; set; } = false;
	[Export] public bool ShouldAttack { get; set; } = false;
	[Export] public bool ShouldSearch { get; set; } = false;
	[Export] public bool ShouldStop { get; set; } = false;

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("monster_alert"))
        {
            GD.Print("Monster alerted is " + !IsAlerted);
            IsAlerted = !IsAlerted;
        }
        if (Input.IsActionJustPressed("damage_monster"))
        {
            TakeDamage(0.9f * MaxHitPoints);
        }
        if (Input.IsActionJustPressed("heal_monster"))
        {
            GD.Print("Healing monster");
            HitPoints = MaxHitPoints;
        }
    }

    public override void _Ready()
    {
        // set up the collision layers and masks
        SetCollisionLayerAndMasks();
    }

    public override void _PhysicsProcess(double delta)
	{
        // find the player controller in the scene tree
        PlayerController playerController = GetTree().Root.GetNode<PlayerController>("GameManager/PlayerController");
        if (playerController != null)
        {
            Vector2 playerPosition = playerController.GlobalPosition;
            var distance = GlobalPosition.DistanceTo(playerPosition);

            processMonsterAction((float)delta, playerController, distance);
        }
        else
        {
            processMonsterAction((float)delta, null, MAX_DEFAULT_DISTANCE);

        }

        Vector2 velocity = Velocity;

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		processMovement((float)delta);

		Velocity = velocity;
		MoveAndSlide();
	}

    private void ClearAllCollisionLayersAndMasks()
    {
        // clear all collision layer assignments
        SetCollisionLayerValue((int)LayerMasks.Player, false);
        SetCollisionLayerValue((int)LayerMasks.WallsAndDoors, false);
        SetCollisionLayerValue((int)LayerMasks.Monster, false);
        SetCollisionLayerValue((int)LayerMasks.ProjectileFriendly, false);
        SetCollisionLayerValue((int)LayerMasks.ProjectileEnemy, false);
        SetCollisionLayerValue((int)LayerMasks.ProjectileOther, false);
        SetCollisionLayerValue((int)LayerMasks.Item, false);
        SetCollisionLayerValue((int)LayerMasks.Interactable, false);
        SetCollisionLayerValue((int)LayerMasks.SpellsFriendly, false);
        SetCollisionLayerValue((int)LayerMasks.SpellsEnemy, false);
        SetCollisionLayerValue((int)LayerMasks.SpellsOther, false);
        SetCollisionLayerValue((int)LayerMasks.NPCs, false);

        // clear all collision masks assignments
        SetCollisionMaskValue((int)LayerMasks.Player, false);
        SetCollisionMaskValue((int)LayerMasks.WallsAndDoors, false);
        SetCollisionMaskValue((int)LayerMasks.Monster, false);
        SetCollisionMaskValue((int)LayerMasks.ProjectileFriendly, false);
        SetCollisionMaskValue((int)LayerMasks.ProjectileEnemy, false);
        SetCollisionMaskValue((int)LayerMasks.ProjectileOther, false);
        SetCollisionMaskValue((int)LayerMasks.Item, false);
        SetCollisionMaskValue((int)LayerMasks.Interactable, false);
        SetCollisionMaskValue((int)LayerMasks.SpellsFriendly, false);
        SetCollisionMaskValue((int)LayerMasks.SpellsEnemy, false);
        SetCollisionMaskValue((int)LayerMasks.SpellsOther, false);
        SetCollisionMaskValue((int)LayerMasks.NPCs, false);

    }

    private void SetCollisionLayerAndMasks()
    {
        //GD.Print("setting spawner collision layer and masks for monster controller");
        // reset the collision layers and masks
        ClearAllCollisionLayersAndMasks();

        // assign our layer
        SetCollisionLayerValue((int)LayerMasks.Monster, true);

        SetCollisionMaskValue((int)LayerMasks.Player, true);
        SetCollisionMaskValue((int)LayerMasks.WallsAndDoors, true);
        SetCollisionMaskValue((int)LayerMasks.Monster, true);
        SetCollisionMaskValue((int)LayerMasks.ProjectileFriendly, true);
        SetCollisionMaskValue((int)LayerMasks.ProjectileOther, true);
        SetCollisionMaskValue((int)LayerMasks.Item, true);
        SetCollisionMaskValue((int)LayerMasks.Interactable, true);
        SetCollisionMaskValue((int)LayerMasks.SpellsFriendly, true);
        SetCollisionMaskValue((int)LayerMasks.SpellsOther, true);
        SetCollisionMaskValue((int)LayerMasks.NPCs, true);
    }

    private void clearAllDistanceStatus()
    {
        IsInAttackRange = false;
        IsInChaseRange = false;
        IsInSearchRange = false;
    }

    private void updateDistanceStatuses(float distance)
    {
        // clear the current status.
        clearAllDistanceStatus();

        if(Math.Abs(distance) < MIN_ATTACK_DISTANCE)
        {
            IsInAttackRange = true;
        } else
        {
            IsInAttackRange = false;
        }

        if(Math.Abs(distance) < MIN_CHASE_DISTANCE)
        {
            IsInChaseRange = true;
        } else
        {
            IsInChaseRange = false;
        }

        if(Math.Abs(distance) < MIN_ATTACK_DISTANCE)
        {
            IsInSearchRange = true;
        } else
        {
            IsInSearchRange = false;
        }
    }

    public void processMonsterAction(float delta, PlayerController player, float distance)
    {

        // if the player is dead let's stop
        if (player == null || player.IsDead)
        {
            // do nothing more
            return;
        }
        else
        {
            if (HitPoints < 0.20 * MaxHitPoints)
            {
                ShouldFlee = true;

                // update the distance status
                updateDistanceStatuses(MAX_DEFAULT_DISTANCE);
            }
            else
            {
                ShouldFlee = false;

                // update the distance status
                updateDistanceStatuses(distance);
            }

            if (ShouldFlee is false)
            {
                //GD.Print("dist to player: " + distance);
                // if it's too close, lets turn away
                if (ShouldFlee is false)
                {
                    if (IsAlerted)
                    {
                        if (IsInChaseRange)
                        {
                            ShouldChasePlayer = true;
                        }

                        if (IsInAttackRange)
                        {
                            ShouldAttack = true;
                        }

                        if (IsInSearchRange)
                        {
                            ShouldSearch = true;
                        }

                        if (HitPoints < 0.20 * MaxHitPoints)
                        {
                            ShouldFlee = true;
                        }
                        else
                        {
                            ShouldFlee = false;
                        }
                    }
                }
            }
            else
            {
                ShouldAttack = false;
                ShouldChasePlayer = false;
                ShouldSearch = false;
                ShouldStop = false;
                ShouldSleep = false;
            }
        }
    }

    public void processMovement(float delta)
	{
        // do no user control movement for now
	}

    public static PackedScene GetScene()
    {
        return GD.Load<PackedScene>(monsterScenePath);

    }

    public virtual void TakeDamage(float damage)
    {
        HitPoints -= damage;
        GD.Print("Monster took damage of " + damage + ". It has " + HitPoints + " left.");

        //GD.Print("Monster took damage");
        if (HitPoints <= 0)
        {
            Die();
        } else
        {
            // monster took damage so now its alert.
            IsAlerted = true;
        }
    }

    public virtual void Knockback(Vector2 direction)
    {
        this.GlobalPosition += direction;
    }


    public virtual void Die()
    {
        GD.Print("--Monster died");

        // TODO:  Award rewards, drop loot, gain experience and so on.
        QueueFree();
    }


    #region State functions
    public void Flee()
	{
        PlayerController playerController = GetTree().Root.GetNode<PlayerController>("GameManager/PlayerController");
        Vector2 direction = playerController.GlobalPosition - this.GlobalPosition;

		//reverse the direction
        Vector2 unit_direction = -direction.Normalized();
        Velocity = unit_direction * Speed;
        //GD.Print("Monster is fleeing");
	}

	public void ChasePlayer()
	{
        PlayerController playerController = GetTree().Root.GetNode<PlayerController>("GameManager/PlayerController");
		Vector2 direction = playerController.GlobalPosition - this.GlobalPosition;
		Vector2 unit_vec = direction.Normalized();
        Velocity = unit_vec * Speed;

        //GD.Print("I'm chasing the player");
	}

	public void Sleep()
	{
		Velocity = new Vector2(0, 0);
        //GD.Print("I'm sleeping");
	}

	public void Attack()
	{
        PlayerController playerController = GetTree().Root.GetNode<PlayerController>("GameManager/PlayerController");
        Vector2 direction = playerController.GlobalPosition - this.GlobalPosition;
        Vector2 unit_vec = direction.Normalized();
        Velocity = unit_vec * (Speed);
        //GD.Print("I'm attacking");
	}

    public void Dead()
    {
		//GD.Print("I'm dead");
    }

    public void Stop()
    {
		//GD.Print("I'm stopped");
        Velocity = new Vector2(0, 0);
    }

    public void Search()
    {
        PlayerController playerController = GetTree().Root.GetNode<PlayerController>("GameManager/PlayerController");
        RandomNumberGenerator rng = new RandomNumberGenerator();
        Vector2 unit_vec = (new Vector2(rng.RandfRange(-1, 1), rng.RandfRange(-1, 1))).Normalized();
        Velocity = unit_vec * (Speed * 0.25f);
        //GD.Print("I'm searching");
    }

    public void Walk()
    {
       //GD.Print("I'm walking");
    }

    public void Idle()
    {
        //GD.Print("I'm idle");
    }
    #endregion
}
