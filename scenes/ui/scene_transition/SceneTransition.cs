using Godot;
using System;
using System.Threading.Tasks;

public partial class SceneTransition : CanvasLayer
{
    [Signal] public delegate void TransitionFinishedEventHandler();
    [Signal] public delegate void SceneFadeInEventHandler();
    [Signal] public delegate void SceneFadeOutEventHandler();


    private static SceneTransition _instance;
    public static SceneTransition Instance => _instance;

    // the setters and getters for our node tree
    AnimationPlayer animationPlayer;

    public override void _Ready()
    {
        // create our setter and getter
        animationPlayer = GetNode<AnimationPlayer>("Control/AnimationPlayer");
    }

    public override void _EnterTree()
    {
        if (_instance != null)
        {
            this.QueueFree(); // The singleton is already loaded, kill this instance
        }
        _instance = this;
    }

    public async Task FadeOut()
    {
        animationPlayer.Play("fade_out");
        await ToSignal(animationPlayer, "animation_finished");
        return;
    }

    public async Task FadeIn()
    {
        animationPlayer.Play("fade_in");
        await ToSignal(animationPlayer, "animation_finished");
        return;
    }

    public void OnAnimationFinished()
    {
        EmitSignal(SignalName.TransitionFinished);
    }
}
