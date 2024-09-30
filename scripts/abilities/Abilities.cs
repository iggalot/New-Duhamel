using Godot;
using System;

public partial class Abilities : Node
{
    public PackedScene BOOMERANG {get; set;}

    public enum AbilitiesList { BOOMERANG, GRAPPLE }

    public AbilitiesList selectedAbility { get; set; } = AbilitiesList.BOOMERANG;
    public PlayerController player { get; set; }

    public Boomerang boomerangInstance { get; set; } = null;

    public override void _Ready()
    {
        BOOMERANG = GD.Load<PackedScene>("res://projectiles/boomerang/boomerang.tscn") as PackedScene;

        player = GlobalPlayerManager.Instance.player;
    }

    public override void _UnhandledInput(InputEvent input_event)
    {
        if (Input.IsActionJustPressed("ability"))
        {
            if (selectedAbility == AbilitiesList.BOOMERANG)
            {
                BoomerangAbility();
            }
        }
        return;
    }

    public void BoomerangAbility()
    {
        // only allow one at a time.
        if (boomerangInstance != null)
        {
            return;
        }

        Boomerang b = BOOMERANG.Instantiate() as Boomerang;
        player.AddChild(b);
        b.GlobalPosition = player.GlobalPosition;

        Vector2 throw_directipon = player.DirectionVector;

        if(throw_directipon == Vector2.Zero)
        {
            throw_directipon = player.CardinalDirection;
        }

        b.Throw(throw_directipon);
        boomerangInstance = b;

        // subscribe to the boomerang destroyed event
        boomerangInstance.BoomerangRemoved += OnBoomerangDestroyed;

        return;
    }

    private void OnBoomerangDestroyed()
    {
        // remove the signal handler and delete the instance here --- since it's not doing it automatically when we QueueFree the boomerang.
        boomerangInstance.BoomerangRemoved -= OnBoomerangDestroyed;
        boomerangInstance = null;
    }
}
