using Godot;
using Godot.Collections;
using System;

public partial class GlobalSaveManager : Node
{
    private static GlobalSaveManager _instance;
    public static GlobalSaveManager Instance => _instance;

    const string SAVE_PATH = "user://";

    [Signal] public delegate void GameLoadedEventHandler();
    [Signal] public delegate void GameSavedEventHandler();

    public static Godot.Collections.Dictionary playerDict;

    public static Godot.Collections.Dictionary itemDict;

    public static Godot.Collections.Dictionary persistenceDict;

    static Godot.Collections.Dictionary questDict;

    Godot.Collections.Dictionary currentSave;

    public override void _Ready()
    {
        playerDict = new Godot.Collections.Dictionary
        {
            { "hp", 1 },
            { "max_hp", 1} ,
            { "pos_x" , 0 },
            { "pos_y" , 0 }
        };

        itemDict = new Godot.Collections.Dictionary
        {
            { "sword", 0 },
            { "axe", 1 },
            { "spear", 2 },
            { "shield", 3 }
        };

        persistenceDict = new Godot.Collections.Dictionary
        {
            { "bless", 0 },
            { "health_up", 1 }
        };

        questDict = new Godot.Collections.Dictionary
        {
            { "quest_1", 0 },
            { "quest_2", 1 },
            { "quest_3", 2 },
            { "quest_4", 3 },
            { "quest_5", 4 }
        };

        currentSave = new Godot.Collections.Dictionary
        {
            { "scene_path", 1 },
            { "player", playerDict },
            { "items", itemDict },
            { "persistence", persistenceDict },
            { "quests", questDict }
        };
    }

    public override void _EnterTree()
    {
        if (_instance != null)
        {
            this.QueueFree(); // The singleton is already loaded, kill this instance
        }
        _instance = this;
    }

    public void SaveGame()
    {
        UpdatePlayerData();
        UpdateScenePath();
        var file = FileAccess.Open( SAVE_PATH + "save.sav", FileAccess.ModeFlags.Write );
        string save_json = Json.Stringify( currentSave );
        file.StoreLine( save_json );
        file.Close(); // remmeber to close the file after writing to it...otherwise changes won't be save.

        EmitSignal(SignalName.GameSaved);
        GD.Print("Saving game...");
        return;
    }

    public async void LoadGame()
    {
        var file = FileAccess.Open(SAVE_PATH + "save.sav", FileAccess.ModeFlags.Read);
        var json = new Json();
        var parseResult = json.Parse(file.GetLine());

        // convert the json data to a dictionary
        Dictionary<string, Variant> data = new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);
        currentSave = (Dictionary)data;

        GlobalLevelManager.Instance.LoadNewLevel(currentSave["scene_path"].ToString(), "", Vector2.Zero);

        // start the level loading (fade to black)
        await ToSignal(GlobalLevelManager.Instance, "LevelLoadStarted");

        // Set the player position
        float pos_x = (float)((Godot.Collections.Dictionary)currentSave["player"])["pos_x"];
        GlobalPlayerManager.Instance.SetPlayerPosition(
            new Vector2(
                (float)((Godot.Collections.Dictionary)currentSave["player"])["pos_x"],
                (float)((Godot.Collections.Dictionary)currentSave["player"])["pos_y"]));

        // set the player health
        GlobalPlayerManager.Instance.SetHealth(
            (float)((Godot.Collections.Dictionary)currentSave["player"])["hp"],
            (float)((Godot.Collections.Dictionary)currentSave["player"])["max_hp"]);

        // finally await for the level to finish loaded
        await ToSignal(GlobalLevelManager.Instance, "LevelLoaded");

        // finally emit our signal that the game has been loaded
        EmitSignal(SignalName.GameLoaded);

        GD.Print("Loading game...");
        return;
    }

    public void UpdatePlayerData()
    {
        PlayerController p = GlobalPlayerManager.Instance.player;

        // this notation is how you retrieve data fro ma subdictionary of a dictionary
        ((Godot.Collections.Dictionary)currentSave["player"])["hp"] = p.HitPoints;
        ((Godot.Collections.Dictionary)currentSave["player"])["max_hp"] = p.MaxHitPoints;
        ((Godot.Collections.Dictionary)currentSave["player"])["pos_x"] = p.GlobalPosition.X;
        ((Godot.Collections.Dictionary)currentSave["player"])["pos_y"] = p.GlobalPosition.Y;
    }

    public void UpdateScenePath()
    {
        string p = string.Empty;

        foreach(Node c in GetTree().Root.GetChildren())
        {
            if(c is Level)
            {
                p = c.SceneFilePath;
                break;
            }
        }

        // this is how you update a key value pair in a dictionary
        currentSave["scene_path"] = p;
    }
}
