using Godot;
using System;

public partial class EnemyCounter : Node2D
{
    [Signal] public delegate void EnemiesDefeatedEventHandler();

    public override void _Ready()
    {
        ChildExitingTree += OnEnemyDestroyed;
        return;
    }

    private void OnEnemyDestroyed(Node enemy)
    {
        if (enemy is MonsterController)
        {
            if(GetEnemyCount() <= 1)
            {
                EmitSignal(SignalName.EnemiesDefeated);
            }
        }
    }


    public int GetEnemyCount()
    {
        int count = 0;
        foreach(Node2D child in GetChildren())
        {
            if(child is MonsterController)
            {
                count += 1;
            }
        }
        return count;
    }
}
