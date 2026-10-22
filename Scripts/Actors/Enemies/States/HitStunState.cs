using Godot;

/// <summary>
/// Estado genérico de aturdimiento al recibir un golpe (sustituye a los *Hit duplicados).
/// Reproduce la animación "hit" y vuelve a "Run" cuando expira el Timer.
/// </summary>
public partial class HitStunState : EnemyState
{
    public override void Enter()
    {
        Enemy.PlayAnimation("hit");
        StartTimerOrDisableFsm();
    }

    public override void Exit() => StopTimer();

    public void OnTimerTimeout()
    {
        if (fsm.currentState == this)
        {
            fsm.TransitionTo("Run");
        }
    }
}
