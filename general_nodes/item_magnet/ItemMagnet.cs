using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ItemMagnet : Area2D
{
    public List<ItemPickup> items { get; set; } = new List<ItemPickup>();
    public List<float> speeds { get; set; } = new List<float>();

    [Export] float magnetStrength = 1.0f;

    [Export] public AudioStreamPlayer2D audio { get; set; }
    [Export] public bool playMagnetAudio { get; set; } = false;
    
    public override void _Ready()
    {
        audio = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");

        AreaEntered += OnAreaEntered;
        return;
    }

    public override void _Process(double delta)
    {
        // run the array in reverse since we are removing items as we go.
        for(int i = items.Count-1; i > -1; i--)
        {
            var _item = items[i];

            if (IsInstanceValid(_item) == true)  // need to check this in case the item was destroyed and this process cycle occurs too fast
            {
                if (_item == null)
                {
                    items.RemoveAt(i);
                    speeds.RemoveAt(i);
                }
                else if (_item.GlobalPosition.DistanceTo(GlobalPosition) > speeds[i])
                {
                    speeds[i] += magnetStrength * (float)delta;
                    _item.Position += _item.GlobalPosition.DirectionTo(GlobalPosition) * speeds[i];
                } else
                {
                    _item.GlobalPosition = GlobalPosition;
                }
            }
        }
    }


    private void OnAreaEntered(Area2D area)
    {
        if(area.GetParent() is ItemPickup)
        {
            ItemPickup new_item = area.GetParent() as ItemPickup;
            items.Add(new_item);

            // make a new items array
            float speed = magnetStrength;
            speeds.Add(speed);
            new_item.SetPhysicsProcess(false);

            if(playMagnetAudio is true)
            {
                audio.Play(0);
            }
        }

        return;
    }
}
