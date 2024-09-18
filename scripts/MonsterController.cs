using Godot;
using scripts.interfaces;
using System;
using static Godot.WebSocketPeer;

public partial class MonsterController : CharacterBody2D
{
    [Signal] public delegate void UpdateHealthBarEventHandler(int health, int mac_health);
    [Signal] public delegate void DirectionChangedEventHandler(Vector2 new_direction);
    [Signal] public delegate void EnemyDamagedEventHandler(float damage);
    [Signal] public delegate void EnemyKilledEventHandler();

    PackedScene monsterScene;
    private static string monsterScenePath = "res://scenes/monster_controller.tscn";

    // the state machine for this monster -- requires state node defintions (if other than the default set).
    private PlayerController player; // store the reference to the player in the scene tree
    private bool isInvulnerable = false;

    // this players properties
    private Vector2 CardinalDirection { get; set; } = Vector2.Down;
    public Vector2[] DIR_4 { get; set; } = new Vector2[] { Vector2.Right, Vector2.Down, Vector2.Left, Vector2.Up };
    public Vector2 DirectionVector { get; set; } = Vector2.Zero;
    public bool IsInvulernable { get; set; } = false;
    public PlayerController Player { get; set; }

    // property flags for moveable pickable and other things -- used by all objects -- sort of an interface hack
    private AttributesManager attributesManager = new AttributesManager();
    
    // Node from Godot scene tree
    private GameManager gameManager;
    public StateMachine stateMachine;
    public AnimationPlayer animationPlayer;
    public Sprite2D sprite;
    public HitBox hitBox;

    private const float default_speed = 100.0f;
    private float friction = 0.25f;
    private float acceleration = 0.3f;




    public float MIN_ATTACK_DISTANCE {get; set;} = 150.0f;
    public float MIN_CHASE_DISTANCE { get; set; } = 300.0f;
    public float MIN_SEARCH_DISTANCE { get; set; } = 400.0f;
    public float MAX_DEFAULT_DISTANCE { get; set; } = 100000.0f;

    public Vector2 DirectionUnitVector { get; set; } = Vector2.Zero;

    // how fast the monster moves in normal movement
    [Export] public float WalkSpeed { get; set; } = 100;
    // how fast the monster moves while searhing
    [Export] public float SearchSpeed { get; set; } = 150.0f;
    // how fast the monster moves while chasing
    [Export] public float ChaseSpeed { get; set; } = 175.0f;
    [Export] public float AttackSpeed { get; set; } = 200.0f;
    // how fast the monster moves while fleeing -- this numbershould be negative to indicate away from the player
    [Export] public float FleeSpeed { get; set; } = -200.0f;
    [Export] public float FleeAtHealthPercentage { get; set; } = 0.25f;


    [Export] public float HitPoints { get; set; } = 100;
    public float MaxHitPoints { get; set; } = 100;
    [Export] public float MeleeDamage { get; set; } = 40.0f;



    [Export] public bool IsAlerted { get; set; } = false;
    [Export] public bool IsDead { get; set; } = false;


    [Export] public bool ShouldFlee { get; set; } = false;

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

            // update the health bar via signal
            EmitSignal(SignalName.UpdateHealthBar, HitPoints, MaxHitPoints);
        }
    }

    public override void _Ready()
    {
        Player = GetTree().Root.GetNode<PlayerController>("GameManager/PlayerController");

        gameManager = GetTree().Root.GetNode<GameManager>("GameManager");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        sprite = GetNode<Sprite2D>("Sprite2D");
        hitBox = GetNode<HitBox>("HitBox");

        // set our Godot node and then intialize the state machine with this as the owner.
        stateMachine = GetNode<StateMachine>("StateMachine");
        stateMachine.Initialize(this);

        // need to tell the individual states who their owner is
        var states = stateMachine.GetChildren();
        foreach (Node state in states)
        {
            State c = state as State;
            if(c == null)
            {
                continue;
            }

            // initialize the owners in each state (so that they are cast correctly)
            // -- this allows us to share the basic statemachine logic aross different entities
            // -- with different state configurations.  This needs to be called after the Initialize() on
            // -- the  state machine.
            c.InitializeOwner();
        }

        // set our global player variable
        Node node = GetTree().Root.GetNode<Node>("PlayerManager");
        GlobalPlayerManager mgr = (GlobalPlayerManager)node;
        mgr.player = GetTree().Root.GetNode<PlayerController>("GameManager/PlayerController");
        player = mgr.player;

        // connect to our HitBox damaged signal
        hitBox.Damaged += TakeDamage;

        // set up the collision layers and masks
        SetCollisionLayerAndMasks();
    }

    public override void _PhysicsProcess(double delta)
    {
        // update velocity by processing the movement of the player
        // -- applying some basic friction and acceleration for moving.
        Velocity = processMovement();



        // if we aren't dead, or the game isn't over, then we can move the player.
        if ((IsDead is true) || (gameManager.IsGameOver is true))
        {
            return;
        } else
        {
            //GD.Print("I'm moving now");
            //GD.Print("My velocity is: " + Velocity);
            //GD.Print("My position is: " + Position);
            MoveAndSlide();
        }
    }

    /// <summary>
    /// handles movement characteristics for the player if it is flagged as moveable
    /// </summary>
    /// <param name="delta"></param>
    /// <returns></returns>
    private Vector2 processMovement()
    {
        Vector2 velocity = this.Velocity;
        //Vector2 dir_unit_vec = new Vector2(0, 0);

        //// is player flagged as moveable?  if so, then handle the movements
        //if (attributesManager.DoPlayerMoveable != null)
        //{
        //    dir_unit_vec = (attributesManager.DoPlayerMoveable).HandleMovement();
        //}

        //// apply friction and acceleration to our velocity
        //if (dir_unit_vec != Vector2.Zero)
        //{
        //    // if e are moving, then apply acceleration
        //    velocity.X = Mathf.Lerp(velocity.X, dir_unit_vec.X * default_speed, acceleration);
        //    velocity.Y = Mathf.Lerp(velocity.Y, dir_unit_vec.Y * default_speed, acceleration);
        //}
        //else
        //{
        //    // otherwise apply friction to slow us down
        //    velocity.X = Mathf.Lerp(velocity.X, 0, friction);
        //    velocity.Y = Mathf.Lerp(velocity.Y, 0, friction);
        //}

        return velocity;
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

    public static PackedScene GetScene()
    {
        return GD.Load<PackedScene>(monsterScenePath);
    }

    public void TakeDamage(float damage)
    {
        // if the monster is invulnerable, don't take damage
        if(IsInvulernable is true)
        {
            return;
        }

        HitPoints -= damage;

        if(HitPoints > 0)
        {
            EmitSignal(SignalName.EnemyDamaged, damage);
        } else
        {
            EmitSignal(SignalName.EnemyKilled);
        }

        //GD.Print("Monster took damage of " + damage + ". It has " + HitPoints + " left.");

        //if(HitPoints < FleeAtHealthPercentage * MaxHitPoints)
        //{
        //    ShouldFlee = true;
        //}

        ////GD.Print("Monster took damage");
        //if (HitPoints <= 0)
        //{
        //    Die();
        //} else
        //{
        //    // monster took damage so now its alert.
        //    IsAlerted = true;
        //}

        //EmitSignal(SignalName.UpdateHealthBar, HitPoints, MaxHitPoints);
    }

    // apply a knockback effect when a monster is hit.
    public virtual void Knockback(Vector2 direction)
    {
        this.GlobalPosition += direction;
    }

    // kill the monster
    public virtual void Die()
    {
        GD.Print("--Monster died");

        DropLoot();

        // TODO:  Award rewards, drop loot, gain experience and so on.
        QueueFree();
    }

    /// <summary>
    /// Spanws look items when a monster dies
    /// TODO:  link loot tables into this.
    /// </summary>
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

    /// <summary>
    /// routine that plays the monster animations associated with each state of the monster.  Called by the
    /// monster states in the state machine.
    /// </summary>
    /// <param name="animation_state_string"></param>
    public void UpdateAnimation(string state)
    {
        // animation
        AnimatedSprite2D statusSprite = GetNode<AnimatedSprite2D>("StatusAnimatedSprite2D") as AnimatedSprite2D;
        AnimationPlayer statusAnimationPlayer = GetNode<AnimationPlayer>("StatusAnimatedSprite2D/StatusAnimationPlayer") as AnimationPlayer;

        // end our current animations and reset them to original
        animationPlayer.Stop();
        animationPlayer.Play("RESET");
        statusAnimationPlayer.Stop();
        statusAnimationPlayer.Play("RESET");

        // set our animation node -- gettin this node doesnt work in Ready() for some reason...but it works here...
        // maybe a timing issue where the player controller isn't full created when READY is called??
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        var state_name = state + "_" + AnimDirection();
        animationPlayer.Play(state + "_" + AnimDirection());

        // play the new animations
        statusSprite.Play(state);
        statusAnimationPlayer.Play("MoveStatusSprite");

        return;
    }

    /// <summary>
    /// routine that plays the monster animations associated with each state of the monster.  Called by the
    /// monster states in the state machine.
    /// </summary>
    /// <param name="animation_state_string"></param>
    public void UpdateStatusSpriteAnimation(string sprite_animation_name)
    {
        // animation
        AnimatedSprite2D statusSprite = GetNode<AnimatedSprite2D>("StatusAnimatedSprite2D") as AnimatedSprite2D;
        AnimationPlayer statusAnimationPlayer = GetNode<AnimationPlayer>("StatusAnimatedSprite2D/StatusAnimationPlayer") as AnimationPlayer;

        // end our current animations and reset them to original
        statusAnimationPlayer.Stop();
        statusAnimationPlayer.Play("RESET");

        // play the new animations
        statusSprite.Play(sprite_animation_name);
        statusAnimationPlayer.Play("MoveStatusSprite");

        return;
    }

    public bool SetDirection(Vector2 new_direction)
    {

        if (DirectionVector == Vector2.Zero)
        {
            return false;
        }

        DirectionVector = new_direction;

        // convoluted method of getting the direction index
        float angle = (DirectionVector.Normalized() + CardinalDirection * 0.1f).Angle();
        float tau_calc = (float)(angle / Math.Tau);
        float times_dir_id = tau_calc * DIR_4.Length;
        var final = (Math.Round(times_dir_id) + DIR_4.Length) % DIR_4.Length; // confine us to a valid index range

        int direction_id = (int)final;
        Vector2 new_dir = DIR_4[direction_id];

        if (new_dir == CardinalDirection)
        {
            return false;
        }

        CardinalDirection = new_dir;
        EmitSignal(SignalName.DirectionChanged, new_dir); // signal to other nodes that the direction has changed

        return true;
    }

    public string AnimDirection()
    {
        if (CardinalDirection == Vector2.Down)
        {
            return "down";
        }
        else if (CardinalDirection == Vector2.Up)
        {
            return "up";
        }
        else if (CardinalDirection == Vector2.Left)
        {
            return "left";
        }
        else
        {
            return "right";
        }
    }
}
