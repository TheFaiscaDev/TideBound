using Godot;
using System;

public partial class PinkyStar_Idle : State
{
    [Export] RayCast2D raycast;
    // El enemigo se obtiene del ControlledNode de la FSM (sin asignar en el inspector)
    private Enemy enemy => GetControlledNode<Enemy>();
    public AnimationNodeStateMachinePlayback animationMachine;


    public override void Enter()
    {
        animationMachine = (AnimationNodeStateMachinePlayback)enemy.animationTree.Get("parameters/playback");
        raycast.Enabled = true;
        animationMachine.Travel("idle");
        Timer timer = GetNodeOrNull<Timer>("Timer");
        if (timer != null)
        {
            timer.Start();
        }
        else
        {
            GD.PushError($"Enemy '{enemy.Name}' requires a Timer node named 'Timer' for the Run state.");
            fsm.TransitionTo("Idle"); // Or an error state
            return;
        }

    }
    public override void Exit()
    {
        // Paramos el timer para que no dispare transiciones fuera del estado
        GetNodeOrNull<Timer>("Timer")?.Stop();
    }

    public override void Update(float delta)
    {

    }
    public override void PhysicsUpdate(float delta)
    {
        if (raycast.IsColliding() && raycast.GetCollider() is Player player)
        {
            fsm.TransitionTo("Attack");
        }

    }
    public override void HandleInput(InputEvent @event) { }

    public void _on_timer_timeout()
    {
        if (fsm.currentState == this)
        {
            fsm.TransitionTo("Run");
        }

    }
}

