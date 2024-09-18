using Godot;
using System.Collections.Generic;

public partial class StateMachine : Node
{
    private List<State> states = new List<State>();
    State previousState;
    State currentState;

    public override void _Ready()
    {
        // delete this ready call until we explictly enable it.
        ProcessMode = Node.ProcessModeEnum.Disabled;
        return;
    }

    public override void _Process(double delta)
    {
        ChangeState(currentState.Process(delta));
        return;
    }

    public override void _PhysicsProcess(double delta)
    {
        ChangeState(currentState.Physics(delta));
        return;
    }

    public override void _UnhandledInput(InputEvent input_event)
    {
        ChangeState(currentState.HandleInput(input_event));
        return;
    }

    public void Initialize(CharacterBody2D character)
    {
        // find out how many children are states (should be all...but you never know)
        foreach (Node c in GetChildren())
        {
            if (c == null)
                continue;

            if(c is State)
            {
                ((State)c).stateOwner = character;
                ((State)c).stateMachine = this;
                ((State)c).Init();
                states.Add((State)c);
            }
        }

        // no states in the list
        if(states.Count == 0)
        {
            return;
        }

        // otherwise

        ChangeState(states[0]);
        ProcessMode = Node.ProcessModeEnum.Inherit; // turn back on the process mode now that we are established
    }

    public void ChangeState(State new_State)
    {
        if ((new_State == null) || (new_State == currentState))
        {
            return;
        }

        if (currentState != null)
        {
            currentState.ExitState();
        }

        previousState = currentState;
        currentState = new_State;
        currentState.EnterState();
    }
}
