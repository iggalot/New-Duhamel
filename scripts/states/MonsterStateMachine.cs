using Godot;
using System;

public partial class MonsterStateMachine : StateMachine
{
    public override void _Ready()
    {
        AddState("idle");
        AddState("sleep");
        AddState("search");
        AddState("chase");
        AddState("attack");
        AddState("flee");
        SetState("idle");
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

        if (State == "search")
        {
            parent.Search();
        } else if (State == "idle")
        {
            if(parent.IsAlerted is true){
                parent.Search();
            } 
            else
            {
                parent.Idle();
            }
        }
        else if (State == "sleep")
        {
            parent.Sleep();
        }

        else if (State == "walk")
        {
            parent.Walk();
        }
        else if (State == "attack")
        {
            parent.Attack();
        } else if (State == "flee")
        {
            parent.Flee();
        } 
        else if (State == "chase")
        {
            parent.ChasePlayer();
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

        if (parent.ShouldFlee is true)
        {
            return "flee";
        }

        switch (State)
        {
            case "sleep":
                if (parent.IsAlerted is true)
                {
                    return "idle";
                } else
                {
                    return "sleep";
                }
            case "idle":

                if (parent.IsAlerted is true)
                {
                    return "search";
                } else if (parent.IsAlerted is false && parent.ShouldSleep is true) {
                    return "sleep";
                } else
                {
                    return "idle";
                }
            case "search":
                if (parent.IsAlerted is true)
                {
                    if(parent.IsInAttackRange is true)
                    {
                        return "attack";
                    }
                    else if (parent.IsInChaseRange is true)
                    {
                        return "chase";
                    }
                    else
                    {
                        return "search";
                    }
                } else
                {
                    return "idle";
                }
            case "chase":
                {
                    if (parent.IsInChaseRange is true)
                    {
                        if (parent.IsInAttackRange is true)
                        {
                            return "attack";
                        } else
                        {
                            return "chase";
                        }
                    } else
                    {
                        return "search";
                    }
                }
            case "attack":
                {
                    if (parent.ShouldAttack is true)
                    {
                        if (parent.IsInAttackRange is true)
                        {
                            return "attack";
                        }
                        else
                        {
                            if (parent.IsInChaseRange is false)
                            {
                                return "search";
                            }
                            else if (parent.IsInChaseRange is true)
                            {
                                return "chase";
                            }
                        }
                    }
                    else
                    {
                        return "idle";
                    }
                    break;
                }

            case "flee":
                if (parent.IsInAttackRange is true)
                {
                    return "attack";
                }
                else if (parent.IsInChaseRange is true)
                {
                    return "chase";
                } 
                else if (parent.IsInSearchRange is true)
                { 
                    return "search";
                } else
                {
                    return "idle";
                }
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

        // animation
        AnimatedSprite2D statusSprite = parent.GetNode<AnimatedSprite2D>("StatusAnimatedSprite2D") as AnimatedSprite2D;
        AnimationPlayer statusAnimationPlayer = parent.GetNode<AnimationPlayer>("StatusAnimatedSprite2D/StatusAnimationPlayer") as AnimationPlayer; 
        AnimationPlayer animationPlayer = parent.GetNode<AnimationPlayer>("AnimationPlayer") as AnimationPlayer;

        // end our current animations and reset them to original
        animationPlayer.Stop();
        animationPlayer.Play("RESET");
        statusAnimationPlayer.Stop();
        statusAnimationPlayer.Play("RESET");

        //switch (new_state)
        //{
        //    case "idle":
        //        {
        //            statusSprite.Play("idle");
        //            break;
        //        }
        //    case "sleep":
        //        {
        //            // play sleep animation here
        //            statusSprite.Play("sleep");
        //            animationPlayer.Play("sleep");
        //            break;
        //        }
        //    case "chase":
        //        {
        //            // play chase animation here
        //            statusSprite.Play("chase");
        //            animationPlayer.Play("chase");
        //            break;
        //        }

        //    case "attack":
        //        {
        //            statusSprite.Play("attack");
        //            animationPlayer.Play("attack");
        //            break;
        //        }
        //    case "flee":
        //        {
        //            statusSprite.Play("flee");
        //            animationPlayer.Play("flee");
        //            break;
        //        }
        //    case "dead":
        //        {
        //            statusSprite.Play("dead");
        //            animationPlayer.Play("dead");
        //            break;
        //        }
        //    case "search":
        //        {
        //            statusSprite.Play("search");
        //            animationPlayer.Play("search");
        //            break;
        //        }
        //    case "stop":
        //        {
        //            statusSprite.Play("stop");
        //            animationPlayer.Play("stop");
        //            break;
        //        }
        //    default:
        //        statusSprite.Play("search");
        //        animationPlayer.Play("search");
        //        break;
        //}

        animationPlayer.Play(new_state);
        statusSprite.Play(new_state);
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
