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

    private string spell_prefix = "fireball";
    Sprite2D spellSprite { get; set; }
    Area2D spellArea { get; set; }
    AnimationPlayer animationPlayer { get; set; }
    [Export] BaseSpellData spellData { get; set; }

    Vector2 initialPosition { get; set; }
    Vector2 currentPosition { get; set; }

    public bool hasImpacted { get; set; }
    private Node2D impactedBody { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        spellArea = GetNode<Area2D>("Area2D");
        spellSprite = GetNode<Sprite2D>("Sprite2D");
        spellData = GetNode<Node>("SpellData") as BaseSpellData;

        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        spellArea.BodyEntered += OnBodyEntered;
        spellArea.BodyExited += OnBodyExited;

        initialPosition = GlobalPosition;
        currentPosition = GlobalPosition;

        spellSprite.Rotation = spellData.SpellDirection.Angle();
    }

    private void OnAnimationFinished(StringName animName)
    {
        throw new NotImplementedException();
    }

    private void OnBodyExited(Node2D body)
    {
        hasImpacted = false;
        GD.Print("spell data body exited -- " + body.Name);
    }

    private void OnBodyEntered(Node2D body)
    {
       
        GD.Print("spell data body entered -- " + body.Name);

        if(body is TileMapLayer)
        {
            hasImpacted = true;
            impactedBody = body as TileMapLayer;

            spellData.SpellDirection = Vector2.Zero;
            spellData.SpellSpeed = 0;
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public async override void _Process(double delta)
    {

        GlobalPosition += (spellData.SpellDirection) * (float)(spellData.SpellSpeed * delta);
        currentPosition = GlobalPosition;

        if (IsInstanceValid(this) is false)
        {
            return;
        }

        if (hasImpacted)
        {
            if(impactedBody is TileMapLayer)
            {
                animationPlayer.Play(spell_prefix +"_impact");
                await ToSignal(animationPlayer, "animation_finished");
                QueueFree();
            }

        } else
        {
            animationPlayer.Play(spell_prefix + "_right");
        }
    }

    public string GetSpellName(SpellsNames spell)
    {
        switch (spell)
        {
            case SpellsNames.SPELL_FIREBALL:
                return "fireball";
            case SpellsNames.SPELL_POISONBALL:
                return "poisonball";
            case SpellsNames.SPELL_LIGHTNING:
                return "lightning";
            case SpellsNames.SPELL_ACIDARROW:
                return "acidarrow";
            case SpellsNames.SPELL_POISONSTREAM:
                return "poisonstream";
            case SpellsNames.SPELL_EARTH:
                return "earth";
            default:
                return "lightning";
        }
    }

}
