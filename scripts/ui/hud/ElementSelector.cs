using Godot;
using Godot.Collections;
using System;

public partial class ElementSelector : Node
{
    private const string ELEMENT_ICON_PATH = "res://resources/graphics/spells/32x32_Moving_Fireball.png";
    private AtlasTexture atlasTexture { get; set; } = new AtlasTexture();

    Dictionary<string, string> elementIconPaths = new Dictionary<string, string>();
    TextureButton elementButton { get; set; }

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
        GD.Print("texture button pressed");
        var region = new Rect2(new Vector2(0, 64), new Vector2(32, 32));
        atlasTexture.Region = region;

        elementButton.TextureNormal = atlasTexture;
    }
}
