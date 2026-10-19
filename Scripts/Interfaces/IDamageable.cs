using Godot;

/// <summary>
/// Cualquier objeto que pueda recibir daño y retroceso (jugador, enemigos, cajas...).
/// Permite atacar de forma type-safe, sin HasMethod/CallDeferred con strings.
/// </summary>
public interface IDamageable
{
    void TakeDamage(int amount);
    void ApplyKnockback(Vector2 sourcePosition, float force, float duration);
}
