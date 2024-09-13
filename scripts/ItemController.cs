using Godot;
using System;

public partial class ItemController : Area2D
{
    PackedScene itemScene;
    private static string itemScenePath = "res://scenes/item_controller.tscn";

    private Area2D itemInteractionArea { get; set; }
	private ColorRect interactionBox { get; set; }

	private bool canInteract { get; set; } = false;

    public static PackedScene GetScene()
    {
        return GD.Load<PackedScene>(itemScenePath);

    }

    public override void _Input(InputEvent @event)
	{
        if (Input.IsKeyPressed(Key.X) && canInteract)
        {
			this.UseItem();

			// test add item to player through game manager
			GameManager _gameManager = GetTree().GetRoot().GetNode("GameManager") as GameManager;

			GD.Print("Add " + this.Name + " to inventory");
			_gameManager.Player.AddItemToInventory(this);

			//GD.Print("Inventory Contents");
			//GD.Print("----------------");
			//GD.Print(_gameManager.Player.DisplayInventory());

			//QueueFree();

   //         GD.Print("Inventory Contents");
   //         GD.Print("----------------");
   //         GD.Print(_gameManager.Player.DisplayInventory());

        }
	}

    private void UseItem()
    {
		GD.Print(" -- Using item");
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		interactionBox = GetNode<ColorRect>("InteractionBox");

		interactionBox.Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnInteractionBodyEntered(CharacterBody2D body)
	{
		GD.Print(body.Name + " has entered the interaction area");
		interactionBox.Visible = true;
		canInteract = true;
	}

    public void OnInteractionBodyExited(CharacterBody2D body)
    {
        GD.Print(body.Name + " has left the interaction area");
		interactionBox.Visible = false;
		canInteract = true;
    }
}
