using Godot;
using Godot.Collections;
using System;

public partial class StateMachine : Node
{
	// an array to hold our states for the state machine
	String[] States = new string[]{};

	//public enum States

	//public enum States
	//{
	//	Null,
	//	None,
	//	Idle,
	//	Walk,
	//	Patrol,
	//	Searching,
	//	Run,
	//	Attack_Melee,
	//	Attack_Range,
	//	Flee,
	//	Rest,
	//	Dead
	//}

	[Export]
	public string State = null;
	public string PreviousState = null;

	public Node Parent { get => GetParent(); }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{

	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        if (State != null)
        {
            StateLogic((float)delta);
            string transition = GetTransition((float)delta);
            if (transition != null)
            {
                SetState(transition);
            }
        }
    }

    public virtual void StateLogic(float delta)
    {
        return;
    }

    public virtual string GetTransition(float delta)
    {
        return null;
    }



	public virtual void EnterState(string new_state, string old_state) 
	{
		return;
	}

	public virtual void ExitState(string old_state, string new_state)
	{
		return;
	}

	public void SetState(string new_state)
    {
        if (State != new_state)
        {
            PreviousState = State;
            State = new_state;
        }

		if(PreviousState != null)
		{
            ExitState(PreviousState, State);
        }
        
		if(new_state != null)
		{
            EnterState(new_state, PreviousState);
        }
    }

	public void AddState(string state)
    {
        if (state != null)
        {
            SetState(state);
            return;
        }

		// resize the current array and add a new state to the list of states
		string[] temp = new string[States.Length + 1];
        System.Array.Copy(States, temp, States.Length);
		States[States.Length] = state;
	}
}
