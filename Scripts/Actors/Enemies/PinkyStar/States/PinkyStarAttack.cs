using Godot;

/// <summary>
/// Ataque de PinkyStar: embestida hacia el jugador. Su daño es por contacto
/// (hurtbox compartida), no por raycast, así que desactiva el golpe por raycast
/// de <see cref="MeleeAttackState"/>.
/// </summary>
public partial class PinkyStarAttack : MeleeAttackState
{
    [Export] public float velocity;

    protected override bool DealsRaycastDamage => false;

    public override void Enter()
    {
        base.Enter();
        velocity = velocity == 0 ? 10.0f : velocity;
    }

    // Embestida: persigue al jugador horizontalmente durante el ataque
    protected override void AttackMotion(float delta)
    {
        if (Enemy.player == null)
        {
            return;
        }
        Vector2 movementDirection = (Enemy.player.GlobalPosition - Enemy.GlobalPosition).Normalized();
        Enemy.Velocity = new Vector2(movementDirection.X * velocity, Enemy.Velocity.Y);
        // La clase base Enemy es la única que llama a MoveAndSlide
    }
}
