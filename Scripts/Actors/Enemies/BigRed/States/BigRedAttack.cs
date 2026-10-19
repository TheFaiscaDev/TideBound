using Godot;
using System;

public partial class BigRedAttack : State
{
    [Export] public RayCast2D raycast;
    // El enemigo se obtiene del ControlledNode de la FSM (sin asignar en el inspector)
    private Enemy enemy => GetControlledNode<Enemy>();

    public override void Enter() {
        enemy.PlayAnimation("attack");
        GetChild<Timer>(0).Start(); // Start the timer for the attack animation
    }
    public override void Exit()
    {
        // Paramos el timer para que no dispare transiciones fuera del estado
        GetNodeOrNull<Timer>("Timer")?.Stop();
    }

    public override void Update(float delta) {
        
    }
    public override void PhysicsUpdate(float delta) {
        if (raycast.IsColliding() && raycast.GetCollider() is Player player && player is IDamageable target)
        {
            target.ApplyKnockback(enemy.GlobalPosition, enemy.PushForce, enemy.PushDuration);
            target.TakeDamage((int)enemy.damage);
        }
      }
    public override void HandleInput(InputEvent @event) { }

    public void OnTimerTimeout()
    {
        if(fsm.currentState == this)
        {
            fsm.TransitionTo("Idle");
        }
    }

}
