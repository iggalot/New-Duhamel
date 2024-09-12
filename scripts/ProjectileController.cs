using Godot;
using System;

public partial class ProjectileController : CharacterBody2D
{
	[Export]
	private ProjectileData _projectileData = new ProjectileData();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	private void ClearAllCollisionLayersAndMasks()
	{
        // clear all collision layer assignments
        SetCollisionLayerValue((int)LayerMasks.Player, false);
        SetCollisionLayerValue((int)LayerMasks.WallsAndDoors, false);
        SetCollisionLayerValue((int)LayerMasks.Monster, false);
        SetCollisionLayerValue((int)LayerMasks.ProjectileFriendly, false);
        SetCollisionLayerValue((int)LayerMasks.ProjectileEnemy, false);
        SetCollisionLayerValue((int)LayerMasks.ProjectileOther, false);
        SetCollisionLayerValue((int)LayerMasks.Item, false);
        SetCollisionLayerValue((int)LayerMasks.Interactable, false);
        SetCollisionLayerValue((int)LayerMasks.SpellsFriendly, false);
        SetCollisionLayerValue((int)LayerMasks.SpellsEnemy, false);
        SetCollisionLayerValue((int)LayerMasks.SpellsOther, false);
        SetCollisionLayerValue((int)LayerMasks.NPCs, false);

        // clear all collision masks assignments
        SetCollisionMaskValue((int)LayerMasks.Player, false);
        SetCollisionMaskValue((int)LayerMasks.WallsAndDoors, false);
        SetCollisionMaskValue((int)LayerMasks.Monster, false);
        SetCollisionMaskValue((int)LayerMasks.ProjectileFriendly, false);
        SetCollisionMaskValue((int)LayerMasks.ProjectileEnemy, false);
        SetCollisionMaskValue((int)LayerMasks.ProjectileOther, false);
        SetCollisionMaskValue((int)LayerMasks.Item, false);
        SetCollisionMaskValue((int)LayerMasks.Interactable, false);
        SetCollisionMaskValue((int)LayerMasks.SpellsFriendly, false);
        SetCollisionMaskValue((int)LayerMasks.SpellsEnemy, false);
        SetCollisionMaskValue((int)LayerMasks.SpellsOther, false);
        SetCollisionMaskValue((int)LayerMasks.NPCs, false);

    }

	private void SetCollisionLayerAndMasks()
	{
		// check that we have data and that this projectile is owneder by someone
		if (_projectileData is null || _projectileData.ProjectileOwner is null)
			return;

        // reset the collision layers and masks
		ClearAllCollisionLayersAndMasks();

		if(_projectileData.ProjectileOwner is PlayerController)
		{
			// assign our layer
			SetCollisionLayerValue((int)LayerMasks.ProjectileFriendly, true);

			SetCollisionMaskValue((int)LayerMasks.WallsAndDoors, true);
            SetCollisionMaskValue((int)LayerMasks.Monster, true);
            SetCollisionMaskValue((int)LayerMasks.Item, true);
            SetCollisionMaskValue((int)LayerMasks.Interactable, true);
            SetCollisionMaskValue((int)LayerMasks.NPCs, true);
        } else if (_projectileData.ProjectileOwner is MonsterController)
		{
            // assign our layer
            SetCollisionLayerValue((int)LayerMasks.ProjectileEnemy, true);

            SetCollisionMaskValue((int)LayerMasks.Player, true);
            SetCollisionMaskValue((int)LayerMasks.WallsAndDoors, true);
            SetCollisionMaskValue((int)LayerMasks.Item, true);
            SetCollisionMaskValue((int)LayerMasks.Interactable, true);
            SetCollisionMaskValue((int)LayerMasks.NPCs, true);
        }

		// let whoever else owns this affect both players and monsters
		else
		{
            // assign our layer
            SetCollisionLayerValue((int)LayerMasks.ProjectileOther, true);

			// assign the collsion masks
            SetCollisionMaskValue((int)LayerMasks.Player, true);
            SetCollisionMaskValue((int)LayerMasks.Monster, true);
            SetCollisionMaskValue((int)LayerMasks.WallsAndDoors, true);
            SetCollisionMaskValue((int)LayerMasks.Item, true);
            SetCollisionMaskValue((int)LayerMasks.Interactable, true);
            SetCollisionMaskValue((int)LayerMasks.NPCs, true);
        }
    }

	public void InitializeData(ProjectileData data)
	{
        _projectileData = data;

        SetCollisionLayerAndMasks();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        // update our current position
        //this._projectileData.ProjectileCurrentPosition = this.GlobalPosition;

        // set our velicity
        //this.Velocity = this._projectileData.ProjectileDirection * ((float)delta * this._projectileData.ProjectileSpeed);
        this.Velocity = this._projectileData.ProjectileDirectionUnitVector * (this._projectileData.ProjectileSpeed);


        var distance_traveled = Math.Abs(this._projectileData.ProjectileSpawnPosition.DistanceTo(this.GlobalPosition));

		//GD.Print(_projectileData.ToString());
       // GD.Print(Velocity);
		//GD.Print("dist traveled: " + distance_traveled);
        //GD.Print("current pos" + this.GlobalPosition);

		if (distance_traveled > this._projectileData.ProjectileRangeDistance)
		{
			GD.Print("Projectile out of range, destroying");
			QueueFree();
		} else
		{
            var collision = MoveAndCollide(Velocity * (float)delta);

            if (collision != null)
            {
                var body = collision.GetCollider();
                GD.Print("Projectile collided with " + body.ToString());
                GD.Print("body's class is: " + body.GetClass());

                if (body is PlayerController)
                {
                    var player = (PlayerController)body;
                    player.TakeDamage(_projectileData.ProjectileDamage);
                    player.Knockback(_projectileData.ProjectileDirectionUnitVector * _projectileData.ProjectileKnockbackDistance);
                } else if (body is MonsterController)
                {
                    var monster = (MonsterController)body;
                    monster.TakeDamage(_projectileData.ProjectileDamage);
                    monster.Knockback(_projectileData.ProjectileDirectionUnitVector * _projectileData.ProjectileKnockbackDistance);
                } else
                {
                    // TODO:
                    GD.Print("decide what to do when an object of type " + body.GetClass() + " is hit by a projectile");
                }

                QueueFree();
            }
        }
    }
}
