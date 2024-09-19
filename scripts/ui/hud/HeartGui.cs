using Godot;
using System;

public partial class HeartGui : Control
{
    // 0 = empty, 1 = half, 2 = full
    private int _value = 2;
    Sprite2D sprite;

    /// <summary>
    /// The current value stored in this Heart
    /// 0 = empty, 1 = half, 2 = full
    /// </summary>
    public int Value {
        get
        {
            return _value;
        }
        set
        {
            _value = value;
            UpdateSprite();
        }
    } 
    
    public override void _Ready()
    {
        sprite = GetNode<Sprite2D>("Sprite2D");
    }

    public void UpdateSprite()
    {
        sprite.Frame = Value;
    }
}
