using Godot;
using System;

public partial class HealthBar : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        Visible = false;
    }

	public void OnUpdateHealthBar(int health, int max_health)
    {
		if(health != max_health)
		{
			Visible = true;
		} else
		{
			// hide the health bar at max health
            Visible = false;
		}

		// set the health bar values
        var health_bar = GetNode<ProgressBar>("ProgressBar");
        health_bar.MaxValue = max_health;
        health_bar.Value = health;
    }
}
