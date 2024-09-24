using Godot;
using System;

public partial class ItemEffectHeal : ItemEffect
{
    [Export] public float healAmount { get; set; } = 20f;
    [Export] public AudioStream audio { get; set; }

    public override void Use()
    {
        GlobalPlayerManager.Instance.player.UpdateHitPoints(healAmount);
        PauseMenu.Instance.PlayAudio(audio);

        // play sound
    }
}
