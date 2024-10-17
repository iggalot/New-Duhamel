using Godot;
using System;

public partial class fire_light : PointLight2D
{
	[Export] NoiseTexture3D noise { get; set; }

	float time_passed = 0.0f;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		time_passed += (float)delta;

		var sampled_noise = noise.Noise.GetNoise1D(time_passed);

		sampled_noise = Math.Abs(sampled_noise);
		//LightEnergy = sampled_noise * 0.25f;

	}
}
