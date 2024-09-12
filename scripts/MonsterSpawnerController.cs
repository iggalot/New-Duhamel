using Godot;
using System;
using System.Collections.Generic;

public partial class MonsterSpawnerController : MonsterController
{
    // timer constants for spawning a new mob
    private float SpawnTimer = 1.0f;
    private float SpawnTimerMax = 1.0f;

    // The distance around the controller for which a mob can be spawned
    private float SpawnRadius = 400.0f;



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
        //GD.Print("Spawning monster");

        //TODO: select what monster to spawn and its stats
        Node root = GetTree().Root;
        Node2D game_mgr = root.GetNode<Node2D>("GameManager");
        Node2D monsters_node = game_mgr.GetNode<Node2D>("Monsters");

        //GD.Print("Monsters in room currently: " + monsters_node.GetChildren().Count);


        // get the room's spawn area
        Area2D spawn_area = game_mgr.GetNode<Area2D>("SpawnArea");

        if(spawn_area == null)
        {
            //GD.Print("Valid spawn area not found.  No monsters being spawned.");
            return;
        }

        CollisionShape2D spawn_area_shape = spawn_area.GetNode<CollisionShape2D>("CollisionShape2D");
        // check that a spawn_area_shape was found
        if(spawn_area_shape == null)
        {
            //GD.Print("Spawn area shape not found");
            return;
        }

        Vector2 spawn_loc = new Vector2(0, 0);

        // otherwise determine the position based on the shape of the spawn area collision box shape
        if(spawn_area_shape.Shape is CircleShape2D)
        {
            GD.Print("Spawn area shape is circle -- not implemented yet");
        } else if (spawn_area_shape.Shape is RectangleShape2D)
        {
            //GD.Print("Spawn area shape is rectangle");
            RectangleShape2D rectangle = (RectangleShape2D)spawn_area_shape.Shape;
            Vector2 origin = spawn_area_shape.GlobalPosition - 0.5f * rectangle.Size; // global pos is at the center points of the collision shap
            Vector2 extents = origin + rectangle.Size; // subtract half of the dimensions from the origina

            // try to spawn a mob at the location
            int spawn_attempt_count = 1;
            bool spawn_success = false;
            while (spawn_attempt_count <= 25)
            {
                // choose a random location within the spawn radius of the spawners origin
                RandomNumberGenerator rng = new RandomNumberGenerator();

                //spawn_loc = new Vector2(rng.RandfRange(origin.X, extents.X), rng.RandfRange(origin.Y, extents.Y));
                spawn_loc = new Vector2(rng.RandfRange(this.GlobalPosition.X - SpawnRadius,
                                                    this.GlobalPosition.X + SpawnRadius),
                                        rng.RandfRange(this.GlobalPosition.Y - SpawnRadius,
                                                    this.GlobalPosition.Y + SpawnRadius));

                // check that we are within the spawn area of the room
                if (spawn_loc.X < origin.X || spawn_loc.X > extents.X || spawn_loc.Y < origin.Y || spawn_loc.Y > extents.Y)
                {
                    // the point is out of bounds so continue to the next iteration
                    spawn_attempt_count++;
                    continue;
                }
                // Otherwise we are within the spawn region for the room so load the monster scene
                else
                {
                    // Create a new monster scnee
                    PackedScene packedScene = MonsterController.GetScene();
                    MonsterController monster = packedScene.Instantiate<MonsterController>() as MonsterController;

                    // check that the monster is in a valid area around the spawner
                    CollisionShape2D monster_shape_body = monster.GetNode<CollisionShape2D>("MonsterBody");
                    Area2D monster_area = monster.GetNode<Area2D>("MonsterSpawnArea");
                    if (spawn_loc.DistanceTo(this.GlobalPosition) < SpawnRadius && monster_area.GetOverlappingBodies().Count == 0)
                    {
                        monster.GlobalPosition = spawn_loc;
                        monsters_node.AddChild(monster);
                        spawn_success = true;
                    }

                    if (spawn_success)
                    {
                        //GD.Print("Monster spawned");
                        break;
                    }
                    else
                    {
                        // Delete the monster since we failed to find a valid spawn point
                        monster.QueueFree();
                    }
                }

                spawn_attempt_count++;
            }


        } else
        {
            GD.Print("Spawn area shape is some other shape -- not implemented yet");
        }




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
