using Godot;
using System;
using System.Collections.Generic;

public partial class StateMachine : Node
{

	[Export] public NodePath InitialState { get; set; }

	[Export] public Node ControlledNode { get; set; }

	private Dictionary<string, State> states;
	public State currentState { get; set; }

	public override void _Ready()
	{
		states = new Dictionary<string, State>();
		foreach (Node node in this.GetChildren())
		{
			if (node is State s)
			{
				states.Add(node.Name, s);
				s.fsm = this;
			}
		}

		currentState = GetNode<State>(InitialState);
		currentState.Enter();
	}

	
	public override void _Process(double delta)
	{
		currentState.Update((float)delta);
	}

	public override void _PhysicsProcess(double delta) {
		currentState.PhysicsUpdate((float)delta);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		currentState.HandleInput(@event);
	}
	
	public void TransitionTo(string key)
	{
		if (!states.ContainsKey(key) || currentState == states[key])
		{
			return;
		}
		currentState.Exit();
		currentState = states[key];
		currentState.Enter();
	}

	/// Vuelve al estado inicial (usado al respawnear al enemigo).
	public void ResetStateMachine()
	{
		if (currentState != null)
		{
			currentState.Exit();
		}
		currentState = GetNode<State>(InitialState);
		currentState.Enter();
	}
}
