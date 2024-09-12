using Godot;
using System;
using System.Collections.Generic;

public partial class MonsterSpawnerController : MonsterController
{
    // timer constants for spawning a new mob
    private float SpawnTimer = 5.0f;
    private float SpawnTimerMax = 5.0f;

    // a list of the monsters that can be spawned from this spawner
    private List<MonsterController> MonsterList = new List<MonsterController>();

    public MonsterSpawnerController()
    {
    }

    public override void _Ready()
    {
        Speed = 0.0f;
        HitPoints = 200;
        MaxHitPoints = 200;

        // set up the collision layers and masks via the base _Ready() class --
        // this function overrides the base _Ready() function in the MonsterController.cs file
        // so we need to explictly call it -- to set up the collision layers and masks
        base._Ready();

    }

    public override void _Process(double delta)
    {
        if (SpawnTimer > 0.0f)
        {
            SpawnTimer -= (float)delta;
        }
        else
        {
            SpawnTimer = SpawnTimerMax;
            SpawnMonster();
        }
    }
    
    private void SpawnMonster()
    {
        GD.Print("Spawning monster");
        GD.Print("MonsterList Count: " + MonsterList.Count);
    }

    // helper function to find a random spawn location near the spawn with in the spawner's radius
    private Vector2 FindSpawnLocation()
    {
        Vector2 spawn_loc = this.GlobalPosition;

        return spawn_loc;
    }

    public override void TakeDamage(float damage)
    {
        HitPoints -= damage;
        GD.Print("Monster spawner took damage of " + damage + ". It has " + HitPoints + " left.");

        //GD.Print("Monster took damage");
        if (HitPoints <= 0)
        {
            Die();
        }
        else
        {
            // monster took damage so now its alert.
            IsAlerted = true;
        }
    }

    public override void Knockback(Vector2 direction)
    {
        // do nothing since this object cant be knocked back
    }


    public override void Die()
    {
        GD.Print("--Monster spawner destroyed");

        // TODO:  Award rewards, drop loot, gain experience and so on.
        QueueFree();
    }
}
