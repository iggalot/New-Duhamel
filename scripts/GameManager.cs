using Godot;

// Payer Layer Mask Definitions
// 1 - Player
// 2 - PlayerHurt
//
// 5 - Walls and Doors
//
// 9 - Monster
// 15 - Projectile - Friendly
// 16 - Projectile - Enemy
// 17 - Projectile - Other
// 20 - Item
// 21 - Interactable
// 25 - Spells - Friendly
// 26 - Spells - Enemy
// 27 - Spells - Other
// 28 - NPCs
// 29 - Background
// 30 - Floors
public enum LayerMasks
{
    Player = 1,
    PlayerHurt = 2,
    WallsAndDoors = 5,
    Monster = 9,
    ProjectileFriendly = 15,
    ProjectileEnemy = 16,
    ProjectileOther = 17,
    Item = 20,
    Interactable = 21,
    SpellsFriendly = 25,
    SpellsEnemy = 26,
    SpellsOther = 27,
    NPCs = 28,
    Background = 29,
    Floors = 30
}


public partial class GameManager : Node2D
{
    public PlayerController Player { get; set; }
    public PlayerHud playerHud { get; set; }


    public bool IsGameOver {get; set;} = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Player = GetNode<PlayerController>("PlayerController");
        playerHud = GetNode<PlayerHud>("PlayerHUD");

        // Set the global variables here
        GlobalPlayerManager.Instance.player = Player;
        GlobalPlayerManager.Instance.playerHud = playerHud;

        ImportData data = new ImportData();
    }

    public override void _Process(double delta)
    {
        if(IsGameOver is true){
            GD.Print("Game Over");
            GetTree().Paused = true; // pause the game loop cause we are dead now

            // TODO:  add game over scene here

            return;
        }

    }
}
