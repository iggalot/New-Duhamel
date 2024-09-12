using Godot;
using System.Collections.Generic;

public partial class CharacterData : Node
{
    [Export] public string CharacterName;
    [Export] public string CharacterDescription;
    [Export] public string CharacterRace;

    [Export] public int CharacterExpValue; // how much this monster is worth when killed.
    [Export] public int CharacterEarnedExp;

    [Export] public int CharacterLevel;
    [Export] public int CharacterExpToNextLevel;
    [Export] public int CharacterMaxLevel;

    [Export] public int CharacterHealth;
    [Export] public int CharacterMaxHealth;
    [Export] public int CharacterStamina;
    [Export] public int CharacterMaxStamina;
    [Export] public int CharacterMana;
    [Export] public int CharacterMaxMana;

    [Export] public int CharacterMeleeAttack;
    [Export] public int CharacterMeleeAttackRadius;
    [Export] public int CharacterMeleeAttackSpeed;
    [Export] public int CharacterDefense;
    [Export] public int CharacterMagicAttack;
    [Export] public int CharacterMagicAttackRadius;
    [Export] public int CharacterMagicAttackSpeed;
    [Export] public int CharacterMagicDefense; // spell suppression base value
    [Export] public int CharacterMovementSpeed;

    // Skill dictionaries for name of skill and skill level in each
    public Dictionary<string, int> CharacterReadySkills { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> CharacterEquippedSkills { get; set; } = new Dictionary<string, int>();

    // Inventory
    public List<ItemController> CharacterInventory { get; set; } = new List<ItemController>();
    public List<ItemController> EquippedItems { get; set; } = new List<ItemController>();


    // Character stats
    [Export] public int STR;
    [Export] public int STR_BONUS;
    [Export] public int DEX;
    [Export] public int AGI;
    [Export] public int INT;
    [Export] public int WIS;
    [Export] public int CHAR;
    [Export] public int CON;
    [Export] public int LCK;

    // Resistances
    [Export] public int FIRE_RES;
    [Export] public int COLD_RES;
    [Export] public int LIGHTNING_RES;
    [Export] public int POISON_RES;
    [Export] public int ARCANE_RES;
    [Export] public int DARK_RES;
    [Export] public int HOLY_RES;
    [Export] public int PHYSICAL_RES;
    [Export] public int EARTH_RES;

    // TODO exp skill curves
}
