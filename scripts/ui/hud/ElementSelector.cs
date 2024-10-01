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

    private const string ELEMENT_ICON_PATH = "res://resources/graphics/spells/32x32_Moving_Fireball.png";
    private AtlasTexture atlasTexture { get; set; } = new AtlasTexture();
    public TextureButton elementButton { get; set; }
    public SpellsNames currentSpell { get; set; }

    public override void _Ready()
    {
        elementButton = GetNode<TextureButton>("HBoxContainer/TextureButton");

        var elementTexture = elementButton.TextureNormal;

        // create the atlas image
        Image atlas_img = new Image();
        atlas_img.Load(ELEMENT_ICON_PATH);
        ImageTexture img_texture = ImageTexture.CreateFromImage(atlas_img);
        atlasTexture.Atlas = img_texture;

        var region = new Rect2(new Vector2(0, 0), new Vector2(32, 32));
        atlasTexture.Region = region;

        elementButton.TextureNormal = atlasTexture;
    }

    public void OnTextureButtonPressed()
    {
        string[] names = Enum.GetNames(typeof(SpellsNames));
        int count = names.Length;
        int next_spell = (int)(currentSpell + 1) % count;
        currentSpell = (SpellsNames)next_spell;

        GD.Print("texture button pressed");
        var region = new Rect2(new Vector2(0, 32 * next_spell), new Vector2(32, 32));
        atlasTexture.Region = region;

        elementButton.TextureNormal = atlasTexture;

        EmitSignal(SignalName.SpellSelected, this);
    }
}
