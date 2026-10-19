using Godot;

/// <summary>
/// Componente de retroceso (knockback) compartido por jugador y enemigos.
/// Calcula la velocidad de retroceso; el CharacterBody2D padre la aplica en su _PhysicsProcess.
/// </summary>
[GlobalClass]
public partial class KnockbackComponent : Node
{
    /// Multiplicador horizontal de la fuerza recibida.
    [Export] public float ForceMultiplier = 3f;

    private CharacterBody2D _body;
    private bool _active;
    private Vector2 _direction = Vector2.Zero;
    private float _force;
    private float _duration;
    private float _timer;

    public bool IsActive => _active;

    public override void _Ready()
    {
        _body = GetParent<CharacterBody2D>();
    }

    public void Apply(Vector2 sourcePosition, float force, float duration)
    {
        if (_body == null) return;
        _direction = (_body.GlobalPosition - sourcePosition).Normalized();
        _force = force;
        _duration = duration;
        _timer = 0f;
        _active = true;
    }

    public void Cancel()
    {
        _active = false;
        _timer = 0f;
    }

    /// Devuelve true si hay retroceso activo y escribe la velocidad a aplicar este frame.
    public bool TryComputeVelocity(double delta, float currentVelocityY, out Vector2 velocity)
    {
        velocity = Vector2.Zero;
        if (!_active || _body == null) return false;

        // El peso del Lerp debe estar en [0, 1]
        float weight = Mathf.Clamp(90f * (float)delta, 0f, 1f);
        velocity = new Vector2(
            _direction.X * _force * ForceMultiplier,
            Mathf.Lerp(currentVelocityY, _direction.Y * _force, weight));

        _timer += (float)delta;
        if (_timer >= _duration)
        {
            Cancel();
            velocity = Vector2.Zero;
        }
        return true;
    }
}
