using Godot;
using System;

public partial class MonsterStateMachine : StateMachine
{
    public override void _Ready()
    {
        AddState("idle");
        AddState("sleep");
        AddState("chase");
        AddState("attack");
        AddState("flee");
        AddState("search");
        SetState("sleep");
        return;
    }   

    /// <summary>
    /// Perform the actions required for each state as defined in the parent controller class
    /// </summary>
    /// <param name="delta"></param>
    public override void StateLogic(float delta)
    {
        // do stuff in the parent class

        MonsterController parent = Parent as MonsterController;

        if (State == "chase")
        {
            parent.ChasePlayer();
        }
        else if (State == "attack")
        {
            parent.Attack();
        } else if (State == "flee")
        {
            parent.Flee();
        } else if (State == "sleep")
        {
            parent.Sleep();
        } else if (State == "search")
        {
            parent.Search();
        }
        else
        {
            parent.Stop();
        }

        // Do other parent state stuff here

        return;
    }

    /// <summary>
    /// Get our transition from one state to the next
    /// </summary>
    /// <param name="delta"></param>
    /// <returns></returns>
    public override string GetTransition(float delta)
    {
        MonsterController parent = Parent as MonsterController;

        switch (State)
        {
            case "sleep":
                if (parent.ShouldChasePlayer is true)
                {
                    return "chase";
                }
                break;
            case "chase":
                if (parent.ShouldSleep is true)
                {
                    return "sleep";
                } else if (parent.ShouldAttack is true)
                {
                    return "attack";
                }
                break;
            case "attack":
                if (parent.ShouldAttack is false)
                {
                    return "sleep";
                }
                break;
            case "flee":
                if(parent.ShouldFlee is false)
                {
                    return "stop";
                }
                break;
            case "stop":
                if(parent.ShouldStop is true)
                {
                    return "search";
                }
                break;
            default:
                break;
        }
        return null;
    }


    /// <summary>
    /// What to do when we enter a state -- usually for firing animations
    /// </summary>
    /// <param name="new_state"></param>
    /// <param name="old_state"></param>
    public override void EnterState(string new_state, string old_state)
    {
        MonsterController parent = Parent as MonsterController;
        GD.Print("my parent is: " + parent.Name);

        // animation
        AnimatedSprite2D statusSprite = parent.GetNode<AnimatedSprite2D>("StatusAnimatedSprite2D") as AnimatedSprite2D;
        AnimationPlayer statusAnimationPlayer = parent.GetNode<AnimationPlayer>("StatusAnimatedSprite2D/StatusAnimationPlayer") as AnimationPlayer; 
        AnimationPlayer animationPlayer = parent.GetNode<AnimationPlayer>("AnimationPlayer") as AnimationPlayer;

        switch (new_state)
        {
            case "sleep":
                {
                    // play sleep animation here
                    statusSprite.Play("sleep");
                    animationPlayer.Play("sleep");
                    break;
                }
            case "chase":
                {
                    // play chase animation here
                    statusSprite.Play("chase");
                    animationPlayer.Play("chase");
                    break;
                }

            case "attack":
                {
                    statusSprite.Play("attack");
                    animationPlayer.Play("attack");
                    break;
                }
            case "flee":
                {
                    statusSprite.Play("flee");
                    animationPlayer.Play("flee");
                    break;
                }
            case "dead":
                {
                    statusSprite.Play("dead");
                    animationPlayer.Play("dead");
                    break;
                }
            case "search":
                {
                    statusSprite.Play("search");
                    animationPlayer.Play("search");
                    break;
                }
            case "stop":
                {
                    statusSprite.Play("stop");
                    animationPlayer.Play("stop");
                    break;
                }
            default:
                statusSprite.Play("search");
                animationPlayer.Play("RESET");
                break;
        }

        statusAnimationPlayer.Play("MoveStatusSprite");
        return;
    }

    /// <summary>
    /// What to do when we exit a state
    /// </summary>
    /// <param name="old_state"></param>
    /// <param name="new_state"></param>
    public override void ExitState(string old_state, string new_state)
    {
        return;
    }
}
