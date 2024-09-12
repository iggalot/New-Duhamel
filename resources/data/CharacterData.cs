using Godot;
using System.Collections.Generic;

public partial class CharacterData : Node
{
    // Base info
    [Export] public string CharacterName;
    [Export] public string CharacterDescription;
    [Export] public string CharacterRace;
    [Export] public string CharacterWeight;
    [Export] public string CharacterHeight;
    [Export] public string CharacterAge;

    // Exp
    [Export] public int CharacterExpValue = 10; // how much this monster is worth when killed.
    [Export] public int CharacterEarnedExp = 0;

    // Level
    [Export] public int CharacterLevel = 1;
    [Export] public int CharacterExpToNextLevel;
    [Export] public int CharacterMaxLevel;

    // Health
    [Export] public int CharacterHealth = 100;
    [Export] public int CharacterMaxHealth = 100;
    [Export] public int CharacterHealthRegeneration = 2;

    // Stamina
    [Export] public int CharacterStamina = 100;
    [Export] public int CharacterMaxStamina = 100;
    [Export] public int CharacterStaminaRegeneration = 2;

    // Mana
    [Export] public int CharacterMana = 100;
    [Export] public int CharacterMaxMana = 100;
    [Export] public int CharacterManaRegeneration = 2;

    // Melee stats
    [Export] public int CharacterMeleeAttack;
    [Export] public string CharacterMeleeAttackType;
    [Export] public int CharacterMeleeAttackRadius;
    [Export] public int CharacterMeleeAttackSpeed;
    [Export] public int CharacterMeleeCritChance;
    [Export] public int CharacterMeleeDefense;

    // Range stats
    [Export] public int CharacterRangedAttack;
    [Export] public string CharacterRangedAttackType;
    [Export] public int CharacterRangedAttackRadius;
    [Export] public int CharacterRangedAttackSpeed;
    [Export] public int CharacterRangedCritChance;
    [Export] public int CharacterRangedDefense;

    // Magic stats
    [Export] public int CharacterMagicAttack;
    [Export] public int CharacterMagicAttackType;
    [Export] public int CharacterMagicAttackRadius;
    [Export] public int CharacterMagicAttackSpeed;
    [Export] public int CharacterMagicCritChance;
    [Export] public int CharacterMagicDefense; // spell suppression base value

    // Other stats
    [Export] public int CharacterMovementSpeed;
    [Export] public float CharacterChaseRange;
    [Export] public float CharacterSearchRange;

    // Skill dictionaries for name of skill and skill level in each
    public Dictionary<string, int> CharacterReadySkills { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, int> CharacterEquippedSkills { get; set; } = new Dictionary<string, int>();

    // Inventory
    public List<ItemController> CharacterInventory { get; set; } = new List<ItemController>();
    public List<ItemController> EquippedItems { get; set; } = new List<ItemController>();
    public int LootChance { get; set; } = 100;
    public int LootTable { get; set; } = 0;


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
    [Export] public int EARTH_RES;
    [Export] public int POISON_RES;
    [Export] public int ARCANE_RES;
    [Export] public int DARK_RES;
    [Export] public int LIGHT_RES;
    [Export] public int NATURE_RES;
    [Export] public int HOLY_RES;
    [Export] public int PHYSICAL_RES;

    // TODO exp and skill curves
}
