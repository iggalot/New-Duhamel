using Godot;
using System;

public partial class MonsterStateMachine : StateMachine
{
    public override void _Ready()
    {
        AddState("sleep");
        AddState("chase");
        AddState("attack");
        AddState("turn");
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

        if(State != "attack" && parent.ShouldFlee is true)
        {
            parent.Flee();
        }

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
        } else
        {
            parent.Sleep();
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
                    return "sleep";
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
        AnimationPlayer animationPlayer = parent.GetNode<AnimationPlayer>("AnimationPlayer") as AnimationPlayer;

        switch (new_state)
        {
            case "sleep":
                {
                    // play sleep animation here
                    animationPlayer.Play("sleep");
                    break;
                }
            case "chase":
                {
                    // play chase animation here
                    animationPlayer.Play("chase");
                    break;
                }

            case "attack":
                {
                    animationPlayer.Play("attack");
                    break;
                }
            case "flee":
                {
                    animationPlayer.Play("turn");
                    break;
                }
            default:
                break;
        }
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
