using Godot;
using scripts.interfaces;

public partial class PlayerController : CharacterBody2D
{
    // property flags for moveable pickable and other things -- used by all objects -- sort of an interface hack
    private AttributesManager attributesManager = new AttributesManager();

    // this players properties
    private const float default_speed = 300.0f;
    private float friction = 0.25f;
    private float acceleration = 0.3f;

    /// <summary>
    /// Inventory stuff
    /// </summary>
    public Inventory PlayerInventory { get; set; } = new Inventory();
    private string inventoryScenePath = "res://scenes/ui/inventory/inventory.tscn";
    private PackedScene inventoryScene;
    private int heldItems = 0;

    // for projectiles
    private string projectileScenePath = "res://scenes/projectile_controller.tscn";
    private PackedScene projectileScene;

    // Node getters and setter
    private AnimatedSprite2D animatedSprite;
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

        // toggles the inventory
        if (Input.IsActionJustPressed("toggle_inventory"))
        {
            PlayerInventory.Visible = !PlayerInventory.Visible;
            PlayerInventory.ShowInventory();
        }

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
        // Set our collision masks
        SetCollisionLayerAndMasks();

        // set up our attributes
        attributesManager.IsPlayerMoveable = true;

        // visuals for the player
        animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        playerMessageWindowAnimationPlayer = GetNode<AnimationPlayer>("PlayerMessageWindow/AnimationPlayer");
        playerMessageWindow = GetNode<ColorRect>("PlayerMessageWindow");
        playerMessageWindow.Visible = false;

        // instantiate the inventory scene and add it as a child to the player controller tree
        inventoryScene = (PackedScene)ResourceLoader.Load(inventoryScenePath);
        Inventory inv_node = inventoryScene.Instantiate() as Inventory;
        PlayerInventory = inv_node;
        AddChild(inv_node);

        // hide the player inventory to start with
        PlayerInventory.Visible = false;
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
        if (IsDead)
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
        //Velocity = playerMovement.HandleMovement((float)delta);
        Velocity = processMovement((float)delta);
        displayMovement();

        if(!IsDead && !IsGameOver) {
            MoveAndSlide();
        }

        // rotate our node so its looking at the mouse position
        //LookAt(GetGlobalMousePosition());

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
    private void displayMovement()
    {
        Vector2 unit_vec = Velocity.Normalized();

        if (unit_vec == Vector2.Zero)
        {
            animatedSprite.Play("none");
        }
        else {
            // returns the angle of the vector between 0 and 360 as a positive degrees with respect to +X <1, 0> vector
            float angle_degrees = (360.0f + Mathf.RadToDeg(unit_vec.Angle())) % 360.0f; ;  

            if (angle_degrees < 45)
            {
                animatedSprite.Play("walk_right");
            } else if (angle_degrees < 135)
            {
                animatedSprite.Play("walk_down");
            } else if (angle_degrees < 225)
            {
                animatedSprite.Play("walk_left");
            } else if (angle_degrees < 315)
            {
                animatedSprite.Play("walk_up");
            } else
            {
                animatedSprite.Play("walk_right");
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
        ItemController new_inv_item = itemController.Duplicate() as ItemController;
        InventorySlot slot = new InventorySlot();
        slot.AddItemToInventorySlot(new_inv_item);
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
        data.ProjectileDirection = (GetGlobalMousePosition() - this.GlobalPosition).Normalized();
        data.ProjectileSpawnPosition = this.GlobalPosition;
        data.ProjectileSize = 10.0f;

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
}
