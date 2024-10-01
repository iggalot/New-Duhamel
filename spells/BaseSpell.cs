using Godot;
using System;

public partial class BaseSpell : Node2D
{
    public enum SpellsNames
    {
        SPELL_FIREBALL = 0,
        SPELL_POISONBALL = 1,
        SPELL_LIGHTNING = 2,
        SPELL_ACIDARROW = 3,
        SPELL_POISONSTREAM = 4,
        SPELL_EARTH = 5
    }



    private const string ELEMENT_ICON_PATH = "res://resources/graphics/spells/32x32_Moving_Fireball.png";
    private AtlasTexture atlasTexture { get; set; } = new AtlasTexture();
    public Texture2D spellTexture { get; set; }

    [Export] public BaseSpellData spellData { get; set; } = new BaseSpellData();


    // nodes for Godot4 tree
    public Sprite2D spellSprite { get; set; }
    public Area2D spellArea { get; set; }
    public AnimationPlayer animationPlayer { get; set; }
    public SpellHurtBox spellHurtBox { get; set; }



    public Vector2 initialPosition { get; set; }
    public Vector2 currentPosition { get; set; }

    public bool hasImpacted { get; set; }
    private Node2D impactedBody { get; set; }

    /// <summary>
    /// Parameterless constructor for Godot4 tree construction
    /// </summary>
    public BaseSpell() 
    {
        // create the basic data -- Initialize should be called to change this after it's been created.
        UpdateSpellTexture(spellData.SpellType);
        spellData.Update();
    }

    /// <summary>
    /// An initializer function for BaseSpell -- to be used after the parameterless constructor has been called
    /// </summary>
    /// <param name="spell_type"></param>
    public void Initialize(SpellsNames spell_type)
    {
        // update the texture first so we can tell if the spell_type is new or not
        UpdateSpellTexture(spell_type);

        spellData.SpellType = spell_type;
        spellData.Update();

    }
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        spellArea = GetNode<Area2D>("Area2D");
        spellSprite = GetNode<Sprite2D>("Sprite2D");
        spellData = GetNode<Node>("SpellData") as BaseSpellData;
        spellHurtBox = GetNode<SpellHurtBox>("SpellHurtBox");

        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        // for detecting if it hits a spell hit box area
        spellArea.AreaEntered += OnAreaEntered;
        spellArea.AreaExited += OnAreaExited;

        // for detecting if it hits a solid body (used for wall detection)
        spellArea.BodyEntered += OnBodyEntered;
        spellArea.BodyExited += OnBodyExited;


        initialPosition = GlobalPosition;
        currentPosition = GlobalPosition;

        spellSprite.Rotation = spellData.SpellDirection.Angle();

        UpdateSpellTexture(spellData.SpellType);
    }


    private void OnBodyExited(Node2D body)
    {
        hasImpacted = false;
        GD.Print("spell body exited -- " + body.Name);
    }

    private void OnAreaExited(Area2D area)
    {
        if(area is SpellHitBox)
        {
            hasImpacted = false;

            GD.Print("spell area exited -- " + area.Name + " in " + area.GetParent().Name);

        }
    }

    private void OnBodyEntered(Node2D body)
    {
        GD.Print("spell body entered -- " + body.Name);

        if (body is TileMapLayer)
        {
            hasImpacted = true;

            if(body is TileMapLayer)
            {
                impactedBody = body as TileMapLayer;

                spellData.SpellDirection = Vector2.Zero;
                spellData.SpellSpeed = 0;
            } 
        }
    }

    private void OnAreaEntered(Area2D area)
    {
        if(area is SpellHitBox)
        {
            GD.Print("-- spell has hit a spell area hit box in " + area.Name + " in " + area.GetParent().Name);
            spellData.SpellDirection = Vector2.Zero;
            spellData.SpellSpeed = 0;
            hasImpacted = true;

            ((SpellHitBox)area).TakeSpellDamage(spellHurtBox);

            //EmitSignal(SignalName.SpellDamaged, area);
        }
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public async override void _Process(double delta)
    {

        GlobalPosition += (spellData.SpellDirection) * (float)(spellData.SpellSpeed * delta);
        currentPosition = GlobalPosition;

        // rotate the spell to align with the direction vector
        spellSprite.Rotation = spellData.SpellDirection.Angle();

        if (IsInstanceValid(this) is false)
        {
            return;
        }

        if (hasImpacted)
        {
            // rotate the spell to align with the direction vector
            spellSprite.Rotation = spellData.SpellDirection.Angle();

            animationPlayer.Play(spellData.SpellName + "_impact");
            await ToSignal(animationPlayer, "animation_finished");
            // now destroy the spell
            QueueFree();
        }
        else
        {
            animationPlayer.Play(spellData.SpellName + "_right");
        }
    }



    public void UpdateSpellTexture(SpellsNames spell_type)
    {
        // if this spell is the same as the previous, don't create a new texture
        //if (spell_type == spellData.SpellType)
        //{
        //    return;
        //}

        spellTexture = CreateTexture(spell_type);
        //spellSprite.Texture = spellTexture;
        spellData.SpellType = spell_type;
    }

    /// <summary>
    /// Creates the spell icon texture from the atlas texture for spells
    /// </summary>
    /// <param name="spell_type"></param>
    /// <returns></returns>
    private Texture2D CreateTexture(SpellsNames spell_type)
    {

        // the position in the atlast texture -- for our base spell sheet, these are all contained in column 0
        int spell_atlas_pos = (int)spell_type;

        // create the atlas image
        Image atlas_img = new Image();
        atlas_img.Load(ELEMENT_ICON_PATH);
        ImageTexture img_texture = ImageTexture.CreateFromImage(atlas_img);
        atlasTexture.Atlas = img_texture;

        var region = new Rect2(new Vector2(0, spell_atlas_pos * 32), new Vector2(32, 32));
        atlasTexture.Region = region;

        return atlasTexture;
    }

}
