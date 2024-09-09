using Godot;
using System;

public partial class ItemController : Area2D
{
	private Area2D itemInteractionArea { get; set; }
	private ColorRect interactionBox { get; set; }

	private bool canInteract { get; set; } = false;

	public override void _Input(InputEvent @event)
	{
        if (Input.IsKeyPressed(Key.X) && canInteract)
        {
			this.UseItem();
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
