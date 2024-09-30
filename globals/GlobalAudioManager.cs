using Godot;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

public partial class GlobalAudioManager : Node
{
    private static GlobalAudioManager _instance;
    public static GlobalAudioManager Instance => _instance;


    public int musicAudioPlayerCounter { get; set; } = 2;
    public int currentMusicPlayer { get; set; } = 0;
    public List<AudioStreamPlayer> musicPlayers { get; set; } = new List<AudioStreamPlayer>();
    public string musicBus { get; set; } = "Music";

    public float musicFadeDuration { get; set; } = 0.5f;

    public override void _Ready()
    {
        ProcessMode = Node.ProcessModeEnum.Always;

        for (int i = 0; i < musicAudioPlayerCounter; i++)
        {
            AudioStreamPlayer player = new AudioStreamPlayer();
            AddChild(player);
            player.Bus = musicBus;
            musicPlayers.Add(player);
            player.VolumeDb = -40;

        }
    }

    public override void _EnterTree()
    {
        if (_instance != null)
        {
            this.QueueFree(); // The singleton is already loaded, kill this instance
        }
        _instance = this;
    }

    public void PlayMusic(AudioStream audio)
    {
        if (audio == musicPlayers[currentMusicPlayer].Stream)
        {
            return;
        } else if (audio == null)
        {
            return;
        }

        currentMusicPlayer += 1;
        if(currentMusicPlayer > 1)
        {
            currentMusicPlayer = 0;
        }

        AudioStreamPlayer current_player = musicPlayers[currentMusicPlayer];

        current_player.Stream = audio;
        PlayAndFadeIn(current_player);

        AudioStreamPlayer oldPlayer = musicPlayers[1];
        if(currentMusicPlayer == 1)
        {
            oldPlayer = musicPlayers[0];
        }

        FadeOutAndStop(oldPlayer);
    }

    public void PlayAndFadeIn(AudioStreamPlayer player)
    {
        player.Play(0);
        Tween tween = CreateTween();
        tween.TweenProperty(player, "volume_db", 0, musicFadeDuration);
    }

    public async void FadeOutAndStop(AudioStreamPlayer player)
    {
        Tween tween = CreateTween();
        tween.TweenProperty(player, "volume_db", -40, musicFadeDuration);
        await ToSignal(tween, Tween.SignalName.Finished);

        player.Stop();
    }
}
