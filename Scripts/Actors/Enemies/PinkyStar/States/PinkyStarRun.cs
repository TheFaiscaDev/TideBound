using Godot;

/// <summary>
/// Patrulla de PinkyStar: toda la lógica vive en <see cref="PatrolPathState"/>.
/// </summary>
public partial class PinkyStarRun : PatrolPathState
{
    // PinkyStar voltea escalando el NODO PADRE del sprite (con Scale.Y = -1),
    // al contrario que BigRed. Es inconsistente con el resto de enemigos
    // (marcado en la revisión de oct 2026); se conserva el comportamiento
    // actual hasta decidir si es intencionado.
    protected override void SetVisualDirection(bool isMovingForward)
    {
        if (Enemy.animatedSprite != null)
        {
            Enemy.animatedSprite.GetParent<Node2D>().Scale = new Vector2(!isMovingForward ? 1 : -1, -1);
        }
    }
}
