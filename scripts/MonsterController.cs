using Godot;
using System;

public partial class MonsterController : CharacterBody2D
{
    PackedScene monsterScene;
    private static string monsterScenePath = "res://scenes/monster_controller.tscn";

    private const float MAX_CHASE_TIMER_DRATION = 5.0f;
    private const float MAX_SEARCH_TIMER_DURATION = 10.0f;


    private const float MIN_ATTACK_DISTANCE = 150.0f;
    private const float MIN_CHASE_DISTANCE = 300.0f;
    private const float MIN_SEARCH_DISTANCE = 400.0f;

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

        DropLoot();

        // TODO:  Award rewards, drop loot, gain experience and so on.
        QueueFree();
    }

    public virtual void DropLoot()
    {
        //GD.Print("Spawning monster");

        //TODO: select what monster to spawn and its stats
        Node root = GetTree().Root;
        Node2D game_mgr = root.GetNode<Node2D>("GameManager");
        Node2D items_node = game_mgr.GetNode<Node2D>("Items");

        //GD.Print("Monsters in room currently: " + monsters_node.GetChildren().Count);


        // get the room's spawn area
        Area2D spawn_area = game_mgr.GetNode<Area2D>("SpawnArea");

        if (spawn_area == null)
        {
            //GD.Print("Valid spawn area not found.  No monsters being spawned.");
            return;
        }

        CollisionShape2D spawn_area_shape = spawn_area.GetNode<CollisionShape2D>("CollisionShape2D");
        // check that a spawn_area_shape was found
        if (spawn_area_shape == null)
        {
            //GD.Print("Spawn area shape not found");
            return;
        }

        Vector2 spawn_loc = this.GlobalPosition;

        // otherwise determine the position based on the shape of the spawn area collision box shape
        if (spawn_area_shape.Shape is CircleShape2D)
        {
            GD.Print("Spawn area shape is circle -- not implemented yet");
        }
        else if (spawn_area_shape.Shape is RectangleShape2D)
        {
            //GD.Print("Spawn area shape is rectangle");
            RectangleShape2D rectangle = (RectangleShape2D)spawn_area_shape.Shape;
            Vector2 origin = spawn_area_shape.GlobalPosition - 0.5f * rectangle.Size; // global pos is at the center points of the collision shap
            Vector2 extents = origin + rectangle.Size; // subtract half of the dimensions from the origina

            //// try to spawn a mob at the location
            int spawn_attempt_count = 1;
            bool spawn_success = false;
            float spawn_radius = 20.0f;
            while (spawn_attempt_count <= 25)
            {
                // choose a random location within the spawn radius of the spawners origin
                RandomNumberGenerator rng = new RandomNumberGenerator();

                //spawn_loc = new Vector2(rng.RandfRange(origin.X, extents.X), rng.RandfRange(origin.Y, extents.Y));
                spawn_loc = new Vector2(rng.RandfRange(this.GlobalPosition.X - spawn_radius,
                                                    this.GlobalPosition.X + spawn_radius),
                                        rng.RandfRange(this.GlobalPosition.Y - spawn_radius,
                                                    this.GlobalPosition.Y + spawn_radius));

                // check that we are within the spawn area of the room
                if (spawn_loc.X < origin.X || spawn_loc.X > extents.X || spawn_loc.Y < origin.Y || spawn_loc.Y > extents.Y)
                {
                    // the point is out of bounds so continue to the next iteration
                    spawn_attempt_count++;
                    continue;
                }
                // Otherwise we are within the spawn region for the room so load the monster scene
                else
                {
                    // Create a new item scnee
                    PackedScene packedScene = ItemController.GetScene();
                    ItemController item = packedScene.Instantiate<ItemController>() as ItemController;

                    // check that the monster is in a valid area around the spawner
                    CollisionShape2D item_shape_body = item.GetNode<CollisionShape2D>("CollisionShape2D");
                    Area2D item_area = item.GetNode<Area2D>("InteractionArea");
                    if (spawn_loc.DistanceTo(this.GlobalPosition) < spawn_radius && item_area.GetOverlappingBodies().Count == 0)
                    {
                        item.GlobalPosition = spawn_loc;
                        items_node.AddChild(item);
                        spawn_success = true;
                    }

                    
                    if (spawn_success)
                    {
                        //GD.Print("Monster spawned");
                        break;
                    }
                    else
                    {
                        // Delete the monster since we failed to find a valid spawn point
                        item.QueueFree();
                    }
                }

                spawn_attempt_count++;
            }


        }
        else
        {
            GD.Print("Spawn area shape is some other shape -- not implemented yet");
        }

        GD.Print("---Spawning loot");
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
