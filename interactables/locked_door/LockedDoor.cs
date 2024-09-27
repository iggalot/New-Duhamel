using Godot;
using System;

public partial class LockedDoor : Node2D

{
    public bool isOpen { get; set; } = false;

    [Export] ItemData keyItem { get; set; }   // what kind of item can open me
    [Export] AudioStream lockedAudio { get; set; }
    [Export] AudioStream openAudio { get; set; }

    public AnimationPlayer animationPlayer { get; set; }
    public AudioStreamPlayer2D audio { get; set; }
    public PersistentDataHandler isOpenData { get; set; }
    public Area2D interactArea { get; set; }

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        audio = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");
        isOpenData = GetNode<PersistentDataHandler>("PersistentDataHandler");
        interactArea = GetNode<Area2D>("InteractArea2D");

        interactArea.AreaEntered += OnAreaEnter;
        interactArea.AreaExited += OnAreaExit;
        isOpenData.DataLoaded += SetState;
        SetState();
    }

    private void OpenDoor()
    {
        if (keyItem == null)
            return;
        bool door_unlocked = GlobalPlayerManager.Instance.INVENTORY_DATA.UseItem(keyItem); ;

        if(door_unlocked is true)
        {
            animationPlayer.Play("open_door");
            audio.Stream = openAudio;
            isOpenData.SetValue();
        } else
        {
            audio.Stream = lockedAudio;
        }

        audio.Play();


        return;
    }


    public void CloseDoor()
    {
        animationPlayer.Play("close_door"); 
    }

    public void SetState()
    {
        isOpen = isOpenData.Value;
        if(isOpen is true)
        {
            animationPlayer.Play("opened");
        } else
        {
            animationPlayer.Play("closed");
        }
    }

    private void OnAreaEnter(Area2D area)
    {
        GlobalPlayerManager.Instance.InteractPressed += OpenDoor;
        return;
    }


    private void OnAreaExit(Area2D area)
    {
        GlobalPlayerManager.Instance.InteractPressed -= OpenDoor;

        return;
    }
}
