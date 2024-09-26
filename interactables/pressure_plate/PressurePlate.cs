using Godot;
using System;

public partial class PressurePlate : Node2D
{
    [Signal] public delegate void ActivatedEventHandler();
    [Signal] public delegate void DeactivatedEventHandler();

    int bodies { get; set; } = 0;
    bool isActive { get; set; } = false;
    Rect2 offRect { get; set; }

    public Area2D area_2d { get; set; }
    public AudioStreamPlayer2D audio { get; set; }
    public AudioStream audio_activate { get; set; }
    public AudioStream audio_deactivate { get; set; }
    public Sprite2D sprite { get; set; }

    public override void _Ready()
    {
        area_2d = GetNode<Area2D>("Area2D");
        audio = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
        sprite = GetNode<Sprite2D>("Sprite2D");
        audio_activate = GD.Load<AudioStream>("res://resources/audio/michael_games/lever-01.wav");
        audio_deactivate = GD.Load<AudioStream>("res://resources/audio/michael_games/lever-02.wav");

        area_2d.BodyEntered += OnBodyEntered;
        area_2d.BodyExited += OnBodyExited;
        offRect = sprite.RegionRect;
    }

    private void OnBodyEntered(Node2D body)
    {
        bodies += 1;  // add to our total bodies counter
        CheckIsActivated();
        return;
    }

    private void OnBodyExited(Node2D body)
    {
        bodies -= 1;
        CheckIsActivated();

        return;
    }

    public void CheckIsActivated()
    {
        if((bodies > 0) && (isActive == false))
        {
            isActive = true;
            float x = offRect.Position.X -32;  // one sprite to the left in the tile map
            float y = offRect.Position.Y;      // same row in the tilemap
            sprite.RegionRect = new Rect2((new Vector2(x, y)), new Vector2(32, 32));  // move the sprite to the depressed sprite
            PlayAudio(audio_activate);

            EmitSignal(SignalName.Activated);
        } else if((bodies <= 0) && (isActive == true))
        {
            isActive = false;
            sprite.RegionRect = offRect;  // move the sprite to the depressed sprite
            PlayAudio(audio_deactivate);

            EmitSignal(SignalName.Deactivated);
        }
    }

    public void PlayAudio(AudioStream stream)
    {
        audio.Stream = stream;
        audio.Play();
    }
}
