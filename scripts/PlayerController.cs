using Godot;
using scripts.interfaces;
using System;

public partial class PlayerController : CharacterBody2D
{
    // property flags for moveable pickable and other things -- used by all objects -- sort of an interface hack
    private AttributesManager attributesManager = new AttributesManager();
    private StateMachine stateMachine;

    private string state = "idle";

    // this players properties
    private Vector2 CardinalDirection { get; set; } = Vector2.Down;
    private Vector2 DirectionVector { get; set; } = Vector2.Zero;

    private const float default_speed = 300.0f;
    private float friction = 0.25f;
    private float acceleration = 0.3f;

    [Export] public Vector2 DirectionUnitVector = Vector2.Zero;
    [Export] public float HitPoints { get; set; } = 100;
    [Export] public float WalkSpeed { get; set; } = default_speed;

    /// <summary>
    /// Inventory stuff
    /// </summary>
    //public Inventory PlayerInventory { get; set; } = new Inventory();
    //private string inventoryScenePath = "res://scenes/ui/inventory/inventory.tscn";
    //private PackedScene inventoryScene;
    //private int heldItems = 0;

    // for projectiles
    private string projectileScenePath = "res://scenes/projectile_controller.tscn";
    private PackedScene projectileScene;

    // Node getters and setter
    private AnimatedSprite2D animatedSprite;
    private Sprite2D sprite;
    private AnimationPlayer animationPlayer;  // for a graphical animation of the character
    private AnimationPlayer playerMessageWindowAnimationPlayer; // for a graphical animation of the player message window
    private ColorRect playerMessageWindow;

    // our attributes
    private float animationTimer = 2.0f;
    private float animationTimerMax = 2.0f;
    private float playerMessageAnimationTimer = 2.0f;
    private float playerMessageAnimationTimerMax = 2.0f;

    // flags for various things
    public bool IsDead = false;
    private bool DeathAnimationHasPlayed = false;
    private bool DeathMessagePopupHasPlayed = false;
    private bool IsGameOver = false;

    public override void _Input(InputEvent @event)
    {
        // a test toggle to kill a player
        if (Input.IsActionJustPressed("dead"))
        {
            // Set isDead to true to test death animation and gamve over cycle
            IsDead = true;
        }

        //// toggles the inventory
        //if (Input.IsActionJustPressed("toggle_inventory"))
        //{
        //    PlayerInventory.Visible = !PlayerInventory.Visible;
        //    PlayerInventory.ShowInventory();
        //}

        // a test toggle to see if we can disable player character movement via the attributes manager
        if (Input.IsActionJustPressed("can_move_toggle"))
        {
            attributesManager.IsPlayerMoveable = !attributesManager.IsPlayerMoveable;
            GD.Print("Player movement allowed: " + attributesManager.IsPlayerMoveable);
        }

        // if the player is shooting
        if (Input.IsActionJustPressed("shoot"))
        {
            Shoot();
        }
    }

    public override void _Ready()
    {
        //// set our Godot node and then intialize the state machine with this as the owner.
        //stateMachine = GetNode<StateMachine>("StateMachine");
        //stateMachine.Initialize(this);

        //// need to tell the individual states who their owner is
        //var states = stateMachine.GetChildren();
        //foreach (var state in states)
        //{
        //    // initialize the owners in each state (so that they are cast correctly)
        //    ((State)state).InitializeOwner();
        //}

        // Set our collision masks
        SetCollisionLayerAndMasks();

        // set up our attributes
        attributesManager.IsPlayerMoveable = true;

        // visuals for the player
        animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        sprite = GetNode<Sprite2D>("Sprite2D");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        playerMessageWindowAnimationPlayer = GetNode<AnimationPlayer>("PlayerMessageWindow/AnimationPlayer");
        playerMessageWindow = GetNode<ColorRect>("PlayerMessageWindow");
        playerMessageWindow.Visible = false;
    }

    public override void _Process(double delta)
    {
        var direction = DirectionVector;
        direction.X = Input.GetActionStrength("right") - Input.GetActionStrength("left");
        //this.Direction.X = Input.GetActionStrength("right") - Input.GetActionStrength("left");
        direction.Y = Input.GetActionStrength("down") - Input.GetActionStrength("up");

        Velocity = direction.Normalized() * WalkSpeed;
        DirectionVector = direction;

        if(SetState() == true || SetDirection() == true)
        {
            UpdateAnimation();
        }

        //if (SetState() is true || SetDirection() is true)
        //{
        //    UpdateAnimation();
        //}

        //// decrease the timers
        //if(playerMessageAnimationTimer > 0.0f && playerMessageAnimationTimer <= playerMessageAnimationTimerMax)
        //{
        //    playerMessageAnimationTimer -= (float)delta;
        //}
        //// is the player dead?  Show the animation then end the game
        //if (IsDead)
        //{
        //    // play the death animation for the character
        //    if (DeathAnimationHasPlayed is false)
        //    {
        //        if (!animationPlayer.IsPlaying() || playerMessageWindowAnimationPlayer.CurrentAnimation != "death")
        //        {
        //            DoAnimationPlayer("death"); // send the death animation to the player -- "death" is defined in Godot's AnimationPlayer
        //            DeathAnimationHasPlayed = true;
        //        }
        //    }

        //    // Display the death message popup
        //    if (DeathMessagePopupHasPlayed is false)
        //    {
        //        if (!playerMessageWindowAnimationPlayer.IsPlaying() || playerMessageWindowAnimationPlayer.CurrentAnimation != "message_window_popup")
        //        {
        //            DoPlayerMessageWindowAnimation("U r ded!"); // display our death message here
        //            DeathMessagePopupHasPlayed  = true;
        //        }
        //    }

        //    // are the animations finished? If so, signal the game has ended and dont let the palyer do anything else
        //    if(DeathMessagePopupHasPlayed && DeathAnimationHasPlayed && !animationPlayer.IsPlaying() && !playerMessageWindowAnimationPlayer.IsPlaying())
        //    {
        //        IsGameOver = true;
        //        return;
        //    }
        //}

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        //Velocity = playerMovement.HandleMovement((float)delta);
        ///Velocity = processMovement((float)delta);



        // rotate our node so its looking at the mouse position
        //LookAt(GetGlobalMousePosition());

    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsDead && !IsGameOver)
        {
            MoveAndSlide();
        }
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
        // reset the collision layers and masks
        ClearAllCollisionLayersAndMasks();

        // assign our layer
        SetCollisionLayerValue((int)LayerMasks.Player, true);

        SetCollisionMaskValue((int)LayerMasks.WallsAndDoors, true);
        SetCollisionMaskValue((int)LayerMasks.Monster, true);
        SetCollisionMaskValue((int)LayerMasks.ProjectileEnemy, true);
        SetCollisionMaskValue((int)LayerMasks.ProjectileOther, true);
        SetCollisionMaskValue((int)LayerMasks.Item, true);
        SetCollisionMaskValue((int)LayerMasks.Interactable, true);
        SetCollisionMaskValue((int)LayerMasks.SpellsEnemy, true);
        SetCollisionMaskValue((int)LayerMasks.SpellsOther, true);
        SetCollisionMaskValue((int)LayerMasks.NPCs, true);
    }


    // handles the animation of our movement
    private string GetAnimatedSpriteString()
    {
        Vector2 unit_vec = Velocity.Normalized();

        if (unit_vec == Vector2.Zero)
        {
            return "none";
        }
        else {
            // returns the angle of the vector between 0 and 360 as a positive degrees with respect to +X <1, 0> vector
            float angle_degrees = (360.0f + Mathf.RadToDeg(unit_vec.Angle())) % 360.0f; ;  

            if (angle_degrees < 45)
            {
                return "walk_right";
            } else if (angle_degrees < 135)
            {
                return "walk_down";
            } else if (angle_degrees < 225)
            {
                return "walk_left";
            } else if (angle_degrees < 315)
            {
                return "walk_up";
            } else
            {
                return "walk_right";
            }
        }
    }

    /// <summary>
    /// handles movement characteristics for the player if it is flagged as moveable
    /// </summary>
    /// <param name="delta"></param>
    /// <returns></returns>
    private Vector2 processMovement(float delta)
    {
        Vector2 velocity = this.Velocity;
        Vector2 dir_unit_vec = new Vector2(0, 0);

        // is player flagged as moveable?  if so, then handle the movements
        if (attributesManager.DoPlayerMoveable != null)
        {
            dir_unit_vec = (attributesManager.DoPlayerMoveable).HandleMovement();
        }

        // apply friction and acceleration to our velocity
        if (dir_unit_vec != Vector2.Zero)
        {
            // if e are moving, then apply acceleration
            velocity.X = Mathf.Lerp(velocity.X, dir_unit_vec.X * default_speed, acceleration);
            velocity.Y = Mathf.Lerp(velocity.Y, dir_unit_vec.Y * default_speed, acceleration);
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

    /// <summary>
    /// Plays a character animation
    /// </summary>
    /// <param name="anim_name"></param>
    private void DoAnimationPlayer(string anim_name)
    {
        if (animationPlayer.IsPlaying() is false && animationPlayer.CurrentAnimation != anim_name)
        {
            animationPlayer.CurrentAnimation = anim_name;
            animationPlayer.Play();
            animationTimer = animationTimerMax;
        }

    }

    /// <summary>
    /// Activates the popup message amimation with the specified message string
    /// </summary>
    /// <param name="message"></param>
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

    /// <summary>
    /// Adds an item to the players inventory
    /// </summary>
    /// <param name="itemController"></param>
    public void AddItemToInventory(ItemController itemController)
    {

    }

    public void Shoot()
    {

        // get the storage groups for the projectiles owned by this player
        var projectiles_node = GetTree().Root.GetNode<Node2D>("GameManager/Projectiles");

        // create the projectile data
        ProjectileData data = new ProjectileData();
        data.ProjectileOwner = this;
        data.ProjectileSpeed = 500.0f;
        data.ProjectileRangeDistance = 3000.0f;
        data.ProjectileDirectionUnitVector = (GetGlobalMousePosition() - this.GlobalPosition).Normalized(); // must be a unit vector
        data.ProjectileSpawnPosition = this.GlobalPosition;
        data.ProjectileSize = 10.0f;
        data.ProjectileDamage = 45.0f;
        data.ProjectileKnockbackDistance = 10.0f;

        GD.Print(data.ToString());

        // instantiate the projectile scene and add it as a child to the player controller tree
        // after initializing data
        projectileScene = (PackedScene)ResourceLoader.Load(projectileScenePath);
        ProjectileController proj_controller_node = projectileScene.Instantiate() as ProjectileController;
        proj_controller_node.InitializeData(data);

        // set the position of this node to be the players glboal position
        proj_controller_node.Position = data.ProjectileSpawnPosition;

        // add the projectile to the scene tree
        projectiles_node.AddChild(proj_controller_node);
    }

    public void Die()
    {
        GD.Print("Player died");
        IsDead = true;
        IsGameOver = true;
        DoAnimationPlayer("death");
    }

    public void TakeDamage(float damage)
    {
        HitPoints -= damage;
        GD.Print("Player took damage of " + damage + ". They have " + HitPoints + " left.");

        if (HitPoints <= 0)
        {
            Die();  
        }
    }

    public void Knockback(Vector2 direction)
    {
        this.GlobalPosition += direction;
    }

    /// <summary>
    /// A helper function to update the animation of our character....
    /// called from the respective states of the state machine
    /// </summary>
    /// <param name="animation_state_string"></param>
    public void UpdatePlayerAnimatedSprite()
    {
        AnimatedSprite2D animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D") as AnimatedSprite2D;
        
        // end our current sprite animations and reset them to original
        animatedSprite.Stop();
        animatedSprite.Play("DEFAULT");

        // play the new animations
        animatedSprite.Play(GetAnimatedSpriteString());
    }

    /// <summary>
    /// A helper function to update the animation of our character....
    /// called from the respective states of the state machine
    /// </summary>
    /// <param name="animation_state_string"></param>
    public void UpdateAnimation(string animation_state_string)
    {
        // animation
        //animationPlayer.Play(animation_state_string);
        animationPlayer.Play(animation_state_string);
    }

    /// <summary>
    /// A helper function to update the animation of our character....
    /// called from the respective states of the state machine
    /// </summary>
    /// <param name="animation_state_string"></param>
    public void UpdateAnimation()
    {
        GD.Print("Updating animation: " + state + "_" + AnimDirection());
        // animation
        //animationPlayer.Play(animation_state_string);
        animationPlayer.Play(state + "_" + AnimDirection());
        return;
    }



    public bool SetDirection()
    {
        Vector2 new_dir = CardinalDirection;

        if(DirectionVector == Vector2.Zero)
        {
            return false;
        }

        if(DirectionVector.Y == 0)
        {
            new_dir = (DirectionVector.X < 0) ? Vector2.Left : Vector2.Right;
        } else if (DirectionVector.X == 0)
        {
            new_dir = (DirectionVector.Y < 0) ? Vector2.Up : Vector2.Down;
        }

        if (new_dir == CardinalDirection)
        {
            return false;
        }

        CardinalDirection = new_dir;

        return true;
    }

    public bool SetState()
    {
        string new_state = (DirectionVector == Vector2.Zero) ? "idle" : "walk";
        if (new_state == state)
        {
            return false;
        }
        state = new_state;
        return true;
    }

    public string AnimDirection()
    {
        if(CardinalDirection == Vector2.Down)
        {
            return "down";
        } else if(CardinalDirection == Vector2.Up)
        {
            return "up";
        } else if (CardinalDirection == Vector2.Left)
        {
            return "left";
        } else
        {
            return "right";
        }
    }
}
