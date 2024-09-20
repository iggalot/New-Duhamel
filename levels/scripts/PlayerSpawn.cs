using Godot;
using System;

public partial class PlayerSpawn : Node2D
{
    public override void _Ready()
    {
        Visible = false;

        if(GlobalPlayerManager.Instance.PlayerSpawned == false)
        {
            GlobalPlayerManager.Instance.SetPlayerPosition(GlobalPosition);
            GlobalPlayerManager.Instance.PlayerSpawned = true;
        }
    }
}
