using Godot;
using System;

public partial class State : Node
{
    public StateMachine fsm;

    /// Nodo controlado por la FSM (asignado en el inspector de la StateMachine).
    public T GetControlledNode<T>() where T : Node
    {
        return fsm.ControlledNode as T;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update(float delta) { }
    public virtual void PhysicsUpdate(float delta) { }
    public virtual void HandleInput(InputEvent @event) { }
}
