using Godot;
using System;

public partial class Hit_BigRed : State
{
    // El enemigo se obtiene del ControlledNode de la FSM (sin asignar en el inspector)
    private Enemy enemy => GetControlledNode<Enemy>();

    public override void Enter() {
        enemy.animatedSprite.Play("hit");
        Timer timer = GetNodeOrNull<Timer>("Timer");
        if (timer != null)
        {
            timer.Start();
        }
    }
    public override void Exit() {
        // Paramos el timer para que no dispare transiciones fuera del estado
        GetNodeOrNull<Timer>("Timer")?.Stop();
    }

    public override void Update(float delta) {

    }
    public override void PhysicsUpdate(float delta) {
        
        
    }
    public override void HandleInput(InputEvent @event) { }

    public void _on_timer_timeout(){
        if(fsm.currentState == this)
        {
            fsm.TransitionTo("Run");
        }
    }
}
