using Godot;
using Godot.Collections;
using System;
using System.Linq;
using static BaseSpell;

public partial class ElementSelector : Node
{
    [Signal] public delegate void SpellSelectedEventHandler(BaseSpell.SpellsNames spell);

    // fireball
    // poisonball
    // lightning
    // acidarrow
    // poisonstream
    // earth


    public TextureButton elementButton { get; set; }
    public BaseSpell currentSpell { get; set; } = new BaseSpell();

    public override void _Ready()
    {
        elementButton = GetNode<TextureButton>("HBoxContainer/TextureButton");

        // set to the fireball instance by default
        currentSpell.Initialize(SpellsNames.SPELL_FIREBALL);

        var elementTexture = elementButton.TextureNormal;
        elementButton.TextureNormal = currentSpell.spellTexture;


    }

    public void OnTextureButtonPressed()
    {
        GD.Print("texture button pressed");

        string[] names = Enum.GetNames(typeof(SpellsNames));
        int count = names.Length;
        int next_spell = (int)(currentSpell.spellData.SpellType + 1) % count;
        currentSpell = new BaseSpell();
        currentSpell.Initialize((SpellsNames)next_spell);
        
        // set the button texture
        elementButton.TextureNormal = currentSpell.spellTexture;

        // signal to GlobalPlayerManager that a new spell has been selected
        EmitSignal(SignalName.SpellSelected, this);
    }
}
