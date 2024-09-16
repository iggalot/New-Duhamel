using Godot;

// Payer Layer Mask Definitions
// 1 - Player
// 2 - Walls and Doors
// 3 - Monster
// 4 - Projectile - Friendly
// 5 - Projectile - Enemy
// 6 - Projectile - Other
// 7 - Item
// 8 - Interactable
// 9 - Spells - Friendly
// 10 - Spells - Enemy
// 11 - Spells - Other
// 12 - NPCs
public enum LayerMasks
{
    Player = 1,
    WallsAndDoors = 2,
    Monster = 3,
    ProjectileFriendly = 4,
    ProjectileEnemy = 5,
    ProjectileOther = 6,
    Item = 7,
    Interactable = 8,
    SpellsFriendly = 9,
    SpellsEnemy = 10,
    SpellsOther = 11,
    NPCs = 11,
    Background = 12,
    Floors = 13
}


public partial class GameManager : Node2D
{
    public PlayerController Player { get; set; }


    public bool IsGameOver {get; set;} = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        Player = GetNode<PlayerController>("PlayerController");

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
