using Godot;
using System;

public partial class GlobalPlayerManager : Node
{
    private ElementSelector _elementSelector;
    public PackedScene PLAYER_SCENE { get; set; }
    public InventoryData INVENTORY_DATA { get; set; } = new InventoryData();

    [Signal] public delegate void InteractPressedEventHandler();
    [Signal] public delegate void SpellSelectorHUDChangedEventHandler(BaseSpell newSpell);

    // a function that can be called from outside of the GlobalPlayerManager to emit a signal for player ineraction pressed
    public void EmitInteractPressedSignal()
    {
        EmitSignal(SignalName.InteractPressed);
    }

    public void UpdateElementSelector(ElementSelector selector)
    {
        OnElementSelectorLoaded(selector);
    }

    private static GlobalPlayerManager _instance;
    public static GlobalPlayerManager Instance => _instance;

    //public InventoryUI inventory { get; set; }

    public PlayerController player { get; set; }
    public PlayerHud playerHud { get; set; }

    public bool PlayerSpawned { get; set; } = false;

    public async override void _Ready()
    {
        // set our player controller scene so we can quickly reload when we transition to other areas
        PLAYER_SCENE = GD.Load<PackedScene>("res://player/player_controller.tscn");

        AddPlayerInstance();

        // set a small delay before we declare a player spawned, in case we are loading from another scene
        await ToSignal(GetTree().CreateTimer(0.2), SceneTreeTimer.SignalName.Timeout);
        PlayerSpawned = true;

        INVENTORY_DATA = GD.Load("res://scenes/ui/inventory/player_inventory.tres") as InventoryData;

        //// hook up to the players spell ability -- default will be lightning
        BaseSpell new_spell = new BaseSpell();
        new_spell.Initialize(BaseSpell.SpellsNames.SPELL_FIREBALL);
        new_spell.spellData.Update();
        //GD.Print("Called for first time from GPM: Ready()");
        SetActiveSpell(new_spell);

        // subscribe to the element changed event in the element selector
        var element_selector = playerHud.GetNode<ElementSelector>("VBoxContainer") as ElementSelector;
        element_selector.ElementSelectorLoaded += OnElementSelectorLoaded;
        //GD.Print("GPM: Connecting to OnElementSelectorLoaded for first time...");
        OnElementSelectorLoaded(element_selector);
    }

    private void OnElementSelectorLoaded(ElementSelector element_selector)
    {
        //GD.Print(" In GPM: OnElementSelectorLoaded");
        //GD.Print("-- before detaching old signals");

        if (IsInstanceValid(_elementSelector))
        {
            //GD.Print("-- old element selector still exists --");
            if(_elementSelector != null)
            {
                //GD.Print("--- old element selector is not null");
                _elementSelector.SpellSelected -= SetActiveSpell;  // detach the old trigger
                _elementSelector.ElementSelectorLoaded -= OnElementSelectorLoaded;

                //GD.Print("---- GPM: Detaching old links to SetActiveSpell");
                //GD.Print("---- GPM: Detaching old links to OnElementSelectorLoaded");

            }
        }

        //GD.Print("-- after detaching old signals");

        // save the new element selector and set the new link to it
        _elementSelector = element_selector;
        // reconnect the signals for when the element selector is reloaded
        _elementSelector.SpellSelected += SetActiveSpell;
        _elementSelector.ElementSelectorLoaded += OnElementSelectorLoaded;

        //GD.Print("---- GPM: Making new links to SetActiveSpell");
        //GD.Print("---- GPM: Making new links to OnElementSelectorLoaded");
    }

    public void AddPlayerInstance()
    {
        player = PLAYER_SCENE.Instantiate() as PlayerController;
        player.ZIndex = 2;
        player.YSortEnabled = true;
        AddChild(player);
        return;
    }

    public override void _EnterTree()
    {
        if(_instance != null)
        {
            this.QueueFree(); // The singleton is already loaded, kill this instance
        }
        _instance = this;
    }

    public void SetHealth(float hp, float max_hp)
    {
        player.MaxHitPoints = max_hp;
        player.HitPoints = hp;
        player.UpdateHitPoints(0); // force an update so that the HUD is updated -- passing a zero so nothing changes
        return;
    }

    public void SetPlayerPosition(Vector2 new_pos)
    {
        player.GlobalPosition = new_pos;
        return;
    }

    public void SetAsParent(Node2D p)
    {
        if (player.GetParent() != null)
        {
            player.GetParent().RemoveChild(player);
        }
        p.AddChild(player);
        return;
    }

    /// <summary>
    /// Removes all active spells case by the player -- useful for when zoning
    /// TODO:: Monsters will need this done too once they are able to cast spells
    /// </summary>
    private void RemoveAllActivePlayerSpells()
    {
        var active_spells = player.GetNode<Node>("ActiveSpells");
        var spells = active_spells.GetChildren();

        for (int i = spells.Count-1; i>=0; i--)
        {
            if (spells[i] is BaseSpell)
            {
                spells[i].QueueFree();
            }
        }
    }
    public void UnparentPlayer(Node2D p)
    {
        RemoveAllActivePlayerSpells();

        if (player.GetParent() != p)
        {
            return;
        }
        p.RemoveChild(player);
        return;
    }

    public void PlayAudio(AudioStream audio)
    {
        player.audio.Stream = audio;
        player.audio.Play();
    }

    public void SetActiveSpell(BaseSpell spell)
    {
        player.activeSpell = spell;
        GD.Print(" -- GPM: SetActiveSpell -- active spell is now: " + player.activeSpell.spellData.SpellName);

        return;
    }
}
