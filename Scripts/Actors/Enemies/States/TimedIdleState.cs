using Godot;

/// <summary>
/// Estado genérico de espera con Timer (sustituye a los antiguos *Idle duplicados).
/// Transiciones: raycast ve al jugador → "Attack"; timeout del Timer → "Run".
/// </summary>
public partial class TimedIdleState : EnemyState
{
    [Export] public RayCast2D raycast;

    public override void Enter()
    {
        raycast.Enabled = true;
        Enemy.PlayAnimation("idle");
        StartTimerOrDisableFsm();
    }

    public override void Exit() => StopTimer();

    public override void PhysicsUpdate(float delta)
    {
        if (raycast.IsColliding() && raycast.GetCollider() is Player)
        {
            fsm.TransitionTo("Attack");
        }
    }

    public void OnTimerTimeout()
    {
        if (fsm.currentState == this)
        {
            fsm.TransitionTo("Run");
        }
    }
}
