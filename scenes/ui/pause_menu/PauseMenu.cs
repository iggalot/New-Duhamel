using Godot;
using System;

public partial class PauseMenu : CanvasLayer
{
    Button buttonSave { get; set; }
    Button buttonLoad { get; set; }

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
        buttonSave = GetNode<Button>("VBoxContainer/Button_Save");
        buttonLoad = GetNode<Button>("VBoxContainer/Button_Load");
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
        buttonSave.GrabFocus(); // focus the attention to the save button
    }

    public void HidePauseMenu()
    {
        GetTree().Paused = false;
        Visible = false;
        IsPaused = false;
    }
}
