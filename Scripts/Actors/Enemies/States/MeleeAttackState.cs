using Godot;

/// <summary>
/// Estado genérico de ataque cuerpo a cuerpo (sustituye a los *Attack duplicados).
/// El daño y el knockback se aplican UNA SOLA VEZ por ataque (no cada frame físico):
/// los i-frames de HealthComponent ya no enmascaran knockback reaplicado constantemente.
/// DealDamage() es público para poder llamarlo desde un Call Method Track del
/// AnimationPlayer en el frame activo de la animación (permite esquivar durante
/// la anticipación y mejora el game feel).
/// Transición: timeout del Timer → "Idle".
/// </summary>
public partial class MeleeAttackState : EnemyState
{
    [Export] public RayCast2D raycast;

    /// Si es true, el estado aplica daño por raycast una vez por ataque.
    /// Enemigos cuyo daño es solo por contacto (hurtbox) deben devolver false.
    protected virtual bool DealsRaycastDamage => true;

    private bool _hasDealtDamage;

    public override void Enter()
    {
        _hasDealtDamage = false;
        Enemy.PlayAnimation("attack");
        StartTimerOrDisableFsm();
    }

    public override void Exit() => StopTimer();

    public override void PhysicsUpdate(float delta)
    {
        AttackMotion(delta);
        if (DealsRaycastDamage)
        {
            DealDamage();
        }
    }

    /// Movimiento durante el ataque (p. ej. embestidas). Por defecto el enemigo no se mueve.
    protected virtual void AttackMotion(float delta) { }

    /// Aplica el golpe UNA única vez por entrada al estado. Llamable desde la animación.
    public void DealDamage()
    {
        if (_hasDealtDamage || raycast == null || !raycast.IsColliding())
        {
            return;
        }
        if (raycast.GetCollider() is Player && raycast.GetCollider() is IDamageable target)
        {
            _hasDealtDamage = true;
            target.ApplyKnockback(Enemy.GlobalPosition, Enemy.PushForce, Enemy.PushDuration);
            target.TakeDamage((int)Enemy.damage);
        }
    }

    public void OnTimerTimeout()
    {
        if (fsm.currentState == this)
        {
            fsm.TransitionTo("Idle");
        }
    }
}
