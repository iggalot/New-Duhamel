using Godot;
using scripts.interfaces;
using System;

public partial class PlayerController : CharacterBody2D
{
    [Signal] public delegate void UpdateHealthBarEventHandler(int health, int mac_health);
    [Signal] public delegate void DirectionChangedEventHandler(Vector2 new_direction);
    [Signal] public delegate void PlayerDamagedEventHandler(HurtBox hurt_box);

    // property flags for moveable pickable and other things -- used by all objects -- sort of an interface hack
    private AttributesManager attributesManager = new AttributesManager();
 //   private GameManager gameManager;

    public PackedScene baseSpellScene { get; set; }
    private string baseSpellScenePath { get; set; } = "res://spells/base_spell_controller.tscn";
    public BaseSpell activeSpell { get; set; }

    // this players properties
    public Vector2 CardinalDirection { get; set; } = Vector2.Down;
    public Vector2[] DIR_4 {get; set;} = new Vector2[]{ Vector2.Right, Vector2.Down, Vector2.Left, Vector2.Up };
    public Vector2 DirectionVector { get; set; } = Vector2.Zero;

    public bool IsInvulnerable { get; set; } = false;

    private const float default_speed = 300.0f;
    private float friction = 0.25f;
    private float acceleration = 0.3f;

    [Export] public float HitPoints { get; set; } = 100;
    [Export] public float MaxHitPoints { get; set; } = 300;
    [Export] public float WalkSpeed { get; set; } = default_speed;
    [Export] public float MeleeDamage { get; set; } = 40.0f;

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
    public AnimationPlayer animationPlayer { get; set; }  // for a graphical animation of the character
    public AnimationPlayer effectAnimationPlayer { get; set; }  // for handling our effect animations (blinks, warps, etc)
    public AnimationPlayer playerMessageWindowAnimationPlayer; // for a graphical animation of the player message window
    public ColorRect playerMessageWindow;
    public HitBox hitBox;
    public Sprite2D sprite;
    public StateMachine stateMachine;
    public AudioStreamPlayer2D audio;
    public Node activeSpellsNode;


    // our attributes
    private float playerMessageAnimationTimer = 2.0f;
    private float playerMessageAnimationTimerMax = 2.0f;

    // flags for various things
    public bool IsDead = false;
    private bool DeathAnimationHasPlayed = false;
    private bool DeathMessagePopupHasPlayed = false;

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
        // get our gameManager object
 //       gameManager = GetTree().Root.GetNode<GameManager>("GameManager");

        // set our global player variable so we can find it in the future from other scripts
        GlobalPlayerManager.Instance.player = this;

        //Node node = GetTree().Root.GetNode<Node>("PlayerManager");
        //GlobalPlayerManager mgr = (GlobalPlayerManager)node;
        //mgr.player = GetTree().Root.GetNode<PlayerController>("GameManager/PlayerController");
        //mgr.player = this;

        // set our Godot node and then intialize the state machine with this as the owner.
        stateMachine = GetNode<StateMachine>("StateMachine");
        stateMachine.Initialize(this);

        // need to tell the individual states who their owner is
        var states = stateMachine.GetChildren();
        foreach (var state in states)
        {
            // initialize the owners in each state (so that they are cast correctly)
            // -- the state machine creates a generic owner, but this function is needed to cast the owner
            // -- to the proper Character2D type (monster controller or play controller) so that the core
            // -- state machine functonality can be shared.
            ((State)state).InitializeOwner();
        }

        // Set our collision masks
        SetCollisionLayerAndMasks();

        // set up our attributes
        attributesManager.IsPlayerMoveable = true;

        // visuals for the player
        sprite = GetNode<Sprite2D>("Sprite2D");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        effectAnimationPlayer = GetNode<AnimationPlayer>("EffectAnimationPlayer");
        playerMessageWindowAnimationPlayer = GetNode<AnimationPlayer>("PlayerMessageWindow/AnimationPlayer");
        playerMessageWindow = GetNode<ColorRect>("PlayerMessageWindow");
        playerMessageWindow.Visible = false;

        hitBox = GetNode<HitBox>("HitBox");
        hitBox.Damaged += TakeDamage;

        audio = GetNode<AudioStreamPlayer2D>("Audio/AudioStreamPlayer2D");

        baseSpellScene = GD.Load<PackedScene>(baseSpellScenePath);
        activeSpellsNode = GetNode("ActiveSpells");
        //// This is a full heal for our player
        //UpdateHitPoints(MaxHitPoints);

        return;
    }

    public override void _Process(double delta)
    {
        // if we are dead, do no further processing
        if (IsDead)
        {
            return;
        }

        // decrease the timers
        if (playerMessageAnimationTimer > 0.0f && playerMessageAnimationTimer <= playerMessageAnimationTimerMax)
        {
            playerMessageAnimationTimer -= (float)delta;
        }

        // need to declare DirectionVector as a variable here for some reason.  Then after the calcs we can
        // assign it back...not sure why that is.
        var direction = DirectionVector;
        //direction.X = Input.GetActionStrength("right") - Input.GetActionStrength("left");
        //direction.Y = Input.GetActionStrength("down") - Input.GetActionStrength("up");
        direction = new Vector2(
            Input.GetAxis("left", "right"),
            Input.GetAxis("up", "down")
        ).Normalized();
        DirectionVector = direction; ;



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

        // rotate our node so its looking at the mouse position
        //LookAt(GetGlobalMousePosition());
    }

    public override void _PhysicsProcess(double delta)
    {
        // update velocity by processing the movement of the player
        // -- applying some basic friction and acceleration for moving.
        Velocity = processMovement();

        MoveAndSlide();
    }

    /// <summary>
    /// handles movement characteristics for the player if it is flagged as moveable
    /// </summary>
    /// <param name="delta"></param>
    /// <returns></returns>
    private Vector2 processMovement()
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
            // otherwise apply friction to slow us down
            velocity.X = Mathf.Lerp(velocity.X, 0, friction);
            velocity.Y = Mathf.Lerp(velocity.Y, 0, friction);
        }

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
        BaseSpell new_spell = (BaseSpell)baseSpellScene.Instantiate();
        new_spell.Initialize(activeSpell.spellData.SpellType);
        new_spell.spellData.Update();

        // Create a unique name for the spell in the tree
        new_spell.Name = activeSpell + "_" + Guid.NewGuid().ToString();

        // set the origination position as the player position
        new_spell.GlobalPosition = GlobalPosition;

        // add the spell to the GODOT scene tree
        activeSpellsNode.AddChild(new_spell);

        // now update the spell data node
        BaseSpellData spell_data_node = new_spell.GetNode("SpellData") as BaseSpellData;

        // create the new data
        BaseSpellData new_spell_data = new BaseSpellData();
        new_spell_data.SpellRange = 500.0f;
        new_spell_data.SpellSpeed = 200.0f;
        new_spell_data.SpellType = activeSpell.spellData.SpellType;
        new_spell_data.Update();  // sets the appropriate name


        // update the spell data in our record (not the tree node)
        new_spell.spellData = new_spell_data;


        Vector2 vec = GetGlobalMousePosition();
        new_spell_data.SpellDirection = (vec - this.GlobalPosition).Normalized();

        // copy the new spell data to the spell data tree node
        spell_data_node.SpellName = new_spell_data.SpellName;
        spell_data_node.SpellRange = new_spell_data.SpellRange;
        spell_data_node.SpellSpeed = new_spell_data.SpellSpeed;
        spell_data_node.SpellType = new_spell_data.SpellType;
        spell_data_node.SpellDirection = new_spell_data.SpellDirection;


        //// get the storage groups for the projectiles owned by this player
        //var projectiles_node = GetTree().Root.GetNode<Node2D>("GameManager/Projectiles");

        //// create the projectile data
        //ProjectileData data = new ProjectileData();
        //data.ProjectileOwner = this;
        //data.ProjectileSpeed = 500.0f;
        //data.ProjectileRangeDistance = 3000.0f;
        //data.ProjectileDirectionUnitVector = (GetGlobalMousePosition() - this.GlobalPosition).Normalized(); // must be a unit vector
        //data.ProjectileSpawnPosition = this.GlobalPosition;
        //data.ProjectileSize = 10.0f;
        //data.ProjectileDamage = 45.0f;
        //data.ProjectileKnockbackDistance = 10.0f;

        //GD.Print(data.ToString());

        //// instantiate the projectile scene and add it as a child to the player controller tree
        //// after initializing data
        //projectileScene = (PackedScene)ResourceLoader.Load(projectileScenePath);
        //ProjectileController proj_controller_node = projectileScene.Instantiate() as ProjectileController;
        //proj_controller_node.InitializeData(data);

        //// set the position of this node to be the players glboal position
        //proj_controller_node.Position = data.ProjectileSpawnPosition;

        //// add the projectile to the scene tree
        //projectiles_node.AddChild(proj_controller_node);
    }

    public void Die()
    {
        GD.Print("-- Player died");
        IsDead = true;
        GameManager.Instance.IsGameOver = true; // signal the game is over
    }

    public void TakeDamage(HurtBox hurt_box)
    { 
        if(IsInvulnerable is true)
        {
            return;
        }
        UpdateHitPoints(-hurt_box.damage);

        GD.Print("Player took damage of " + hurt_box.damage + ". You have " + HitPoints + " left.");


        //GD.Print("Monster took damage");
        if (HitPoints > 0)
        {
            EmitSignal(SignalName.PlayerDamaged, hurt_box);
        } else
        {
            GD.Print("Player died");
            // for now do a full heal on the player
            EmitSignal(SignalName.PlayerDamaged, hurt_box);
            UpdateHitPoints(MaxHitPoints);  // instant heal if we die for now
        }

        EmitSignal(SignalName.UpdateHealthBar, HitPoints, MaxHitPoints);
    }

    public void UpdateHitPoints(float delta)
    {
        HitPoints = Math.Clamp(HitPoints + delta, 0, MaxHitPoints);

        if (GlobalPlayerManager.Instance.playerHud != null)
            GlobalPlayerManager.Instance.playerHud.UpdateHitPoints(HitPoints, MaxHitPoints);
        return;
    }

    public async void MakeInvulnerable(float duration = 1.0f)
    {
        IsInvulnerable = true;
        hitBox.Monitoring = false;

        // cause a delay before we can be hit again
        await ToSignal(GetTree().CreateTimer(duration), SceneTreeTimer.SignalName.Timeout);

        IsInvulnerable = false;
        hitBox.Monitoring = true;
        return;
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
    public void UpdateAnimation(string state)
    {
        // set our animation node -- gettin this node doesnt work in Ready() for some reason...but it works here...
        // maybe a timing issue where the player controller isn't full created when READY is called??
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        animationPlayer.Play(state + "_" + AnimDirection());
        return;
    }


    public bool SetDirection()
    {
        //GD.Print("DirectionVector" + DirectionVector);
        if(DirectionVector == Vector2.Zero)
        {
            return false;
        }

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
