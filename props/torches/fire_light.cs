using Godot;
using System;

/// <summary>
/// This script controls the flickering of a PointLight2D child.
/// </summary>
public partial class fire_light : Node2D
{
	private float start_energy =  3.0f;
	private float incremental_energy = 0.5f;
	private float start_frequency = 5.0f;
	private float start_texture_scale = 1.25f;
	private float max_texture_scale = 1.1f;
	PointLight2D light { get; set; }

	float time_passed = 0.0f;

    public override void _Ready()
    {
		// set the getter for the light node
		light = GetNode<PointLight2D>("PointLight2D");
		light.Energy = start_energy;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		time_passed += (float)delta;

		var rng = new RandomNumberGenerator();
		float rand = rng.RandfRange(0.9f, 1.1f);
        float rand_frequency = (1+(float)Math.Abs(Math.Sin(time_passed * start_frequency)));

        float rand_texture_scale = (float)Math.Abs(Math.Sin(time_passed * start_frequency * rand) * rand_frequency);
		rand_texture_scale = Math.Min(rand_texture_scale, max_texture_scale);  // clips the upper limit
		//GD.Print(rand_texture_scale);

        float frequency = 0.0f * rand_frequency;
        float change =  incremental_energy * rand * frequency;
		light.Energy = (float)start_energy + change;

		light.TextureScale = (start_texture_scale + rand_texture_scale *0.1f) * 0.5f;
	}
}
