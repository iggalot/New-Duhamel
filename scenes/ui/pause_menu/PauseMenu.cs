using Godot;
using System;

public partial class PauseMenu : CanvasLayer
{
    [Signal] public delegate void ShownEventHandler();
    [Signal] public delegate void HiddenEventHandler();

    // nodes on our tree
    AudioStreamPlayer audioStreamPlayer { get; set; }
    Button buttonSave { get; set; }
    Button buttonLoad { get; set; }
    Label itemDescription { get; set; }



    bool IsPaused = false;

    private static PauseMenu _instance;
    public static PauseMenu Instance => _instance;

    public override void _EnterTree()
    {
        if (_instance != null)
        {
            this.QueueFree(); // The singleton is already loaded, kill this instance
        }
        _instance = this;
    }


    public override void _Ready()
    {
        HidePauseMenu();  // hide the menu as soon as this node loads

        // setup the getters for our button nodes
        audioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        buttonSave = GetNode<Button>("Control/HBoxContainer/Button_Save");
        buttonLoad = GetNode<Button>("Control/HBoxContainer/Button_Load");
        itemDescription = GetNode<Label>("Control/ItemDescription");

        buttonSave.Pressed += OnSavePressed;
        buttonLoad.Pressed += OnLoadPressed;
    }

    public override void _UnhandledInput(InputEvent this_event)
    {
        if (this_event.IsActionPressed("pause"))
        {
            if(IsPaused == false)
            {
                ShowPauseMenu();
            } else
            {
                HidePauseMenu();
            }

            GetViewport().SetInputAsHandled();
        }
    }

    public void ShowPauseMenu()
    {
        GetTree().Paused = true;
        Visible = true;
        IsPaused = true;
        EmitSignal(SignalName.Shown);
    }

    public void HidePauseMenu()
    {
        GetTree().Paused = false;
        Visible = false;
        IsPaused = false;
        EmitSignal(SignalName.Hidden);
    }

    public void OnSavePressed()
    {
        if(IsPaused == false)
        {
            return;
        }
        GlobalSaveManager.Instance.SaveGame();
        HidePauseMenu();
    }

    public async void OnLoadPressed()
    {
        if(IsPaused == false)
        {
            return;
        }
        GlobalSaveManager.Instance.LoadGame();
        
        await ToSignal(GlobalLevelManager.Instance, "LevelLoadStarted");

        HidePauseMenu();
    }

    public void UpdateItemDescription(string new_text)
    {
        itemDescription.Text = new_text;
    }

    public void PlayAudio(AudioStream audio)
    {
        audioStreamPlayer.Stream = audio;
        audioStreamPlayer.Play();
    }
}
