using Godot;
using System;

public partial class Level : Node2D
{
    public override void _Ready()
    {
        this.YSortEnabled = true;
        GlobalPlayerManager.Instance.SetAsParent(this);
        return;
    }
}
