using Godot;
using Godot.Collections;
using System;
using System.Linq;
using static BaseSpell;

public partial class ElementSelector : Node
{
    [Signal] public delegate void SpellSelectedEventHandler(BaseSpell spell);

    // signal to other nodes that the element selector has been loaded.
    [Signal] public delegate void ElementSelectorLoadedEventHandler(ElementSelector selector);

    // fireball
    // poisonball
    // lightning
    // acidarrow
    // poisonstream
    // earth


    public TextureButton elementButton { get; set; }
    public BaseSpell currentSpell { get; set; } = new BaseSpell();

    public override void _Ready()
    {
        elementButton = GetNode<TextureButton>("HBoxContainer/TextureButton");

        // set to the fireball instance by default
        currentSpell.Initialize(SpellsNames.SPELL_FIREBALL);
        EmitSignal(SignalName.SpellSelected, currentSpell);

        var elementTexture = elementButton.TextureNormal;
        elementButton.TextureNormal = currentSpell.spellTexture;

        // establishes the  signals and connections in GPM to this element select in GPM.
        GlobalPlayerManager.Instance.UpdateElementSelector(this);  

        // connect to the HUD changed signal
        GlobalPlayerManager.Instance.SpellSelectorHUDChanged += OnSpellSelectorChanged;

        //GD.Print("-- ES is connecting to GPM SpellSelectorHUDChanged");
        OnSpellSelectorChanged(currentSpell);  // call for an update

        // signal that the element selector is loaded now
        EmitSignal(SignalName.ElementSelectorLoaded, this);
        //GD.Print("-- ES is emitting ElementSelectorLoaded");
    }

    /// <summary>
    /// Routine to update the texture of the selector button for a spell change
    /// </summary>
    /// <param name="new_spell"></param>
    private void OnSpellSelectorChanged(BaseSpell new_spell)
    {
        // has the global player manager been initialized?
        if(GlobalPlayerManager.Instance.player == null)
        {
            return;
        }

        // initialize the spell to something if its not already set
        if(currentSpell == null)
        {
            currentSpell.Initialize(SpellsNames.SPELL_FIREBALL);
        }

        BaseSpell player_spell = GlobalPlayerManager.Instance.player.activeSpell;

        // does the player currently have a spell assigned to them that is different than the selector?
        // if so, update the selector;  
        // currentSpell will never be negative here -- default cause it gets assigned as Fireball
        if(player_spell == null)
        {
            player_spell = currentSpell;
        } else
        {
            currentSpell = player_spell;
        }

        // is the currently selected spell on the HUD the same as what was assigned to the activeSpell of the
        // player controller?  Is they are the same, no need to change anything
        if (currentSpell == new_spell)
        {
            return;
        }

        // set the button texture
        elementButton.TextureNormal = currentSpell.spellTexture;

        // signal to GlobalPlayerManager that a new spell has been selected, so we can update the player character
        // emitting this signal again will force a call back into this function from GlobalPlayerManager
        EmitSignal(SignalName.SpellSelected, this.currentSpell);
        //GD.Print("-- ES: is emitting SpellSelected from OnSpellSelectorChanged");
    }

    /// <summary>
    /// What to do if the spell selector button has been clicked.  Currently rotates through the 
    /// entire list of define spells.
    /// TODO:  This may need to be changed at some point.
    /// </summary>
    public void OnTextureButtonPressed()
    {
        //GD.Print("texture button pressed");

        string[] names = Enum.GetNames(typeof(SpellsNames));
        int count = names.Length;
        int next_spell = (int)(currentSpell.spellData.SpellType + 1) % count;
        currentSpell = new BaseSpell();
        currentSpell.Initialize((SpellsNames)next_spell);
        
        // set the button texture
        elementButton.TextureNormal = currentSpell.spellTexture;

        // signal to GlobalPlayerManager that a new spell has been selected
        //GD.Print("-- ES is emitting SpellSelected from OnTextureButtonPressed ");
        EmitSignal(SignalName.SpellSelected, this.currentSpell);
    }
}
