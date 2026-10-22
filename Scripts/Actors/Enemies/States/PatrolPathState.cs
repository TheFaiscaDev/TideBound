using Godot;

/// <summary>
/// Estado genérico de patrulla sobre un Path2D/PathFollow2D (sustituye a los
/// antiguos BigRedRun/PinkyStarRun duplicados).
/// Requiere: el enemigo es hijo directo de un PathFollow2D con un Path2D con curva válida,
/// y un hijo Timer llamado "Timer" (timeout → OnTimerTimeout → "Idle").
/// Transiciones: raycast ve al jugador → "Attack"; timeout del Timer → "Idle".
/// </summary>
public partial class PatrolPathState : EnemyState
{
    [Export] public float speed = 100f; // Pixels per second
    [Export] public RayCast2D raycast;

    private bool movingForward = true;
    private PathFollow2D pathFollow;
    private Path2D path;

    // Tolerancia para comparaciones de flotantes en los extremos del path
    private const float PATH_END_TOLERANCE = 0.005f;

    public override void Enter()
    {
        if (!StartTimerOrDisableFsm())
        {
            return;
        }

        pathFollow = Enemy.GetParent<PathFollow2D>();
        if (pathFollow == null)
        {
            GD.PushError($"Enemy '{Enemy.Name}' requiere un PathFollow2D como padre directo.");
            fsm.TransitionTo("Idle");
            return;
        }

        path = pathFollow.GetParent<Path2D>();
        if (path == null || path.Curve == null)
        {
            GD.PushError($"El PathFollow2D del enemigo '{Enemy.Name}' requiere un Path2D con un recurso Curve válido como padre.");
            fsm.TransitionTo("Idle");
            return;
        }

        if (path.Curve.GetBakedLength() <= 0f)
        {
            GD.PushWarning($"El path del enemigo '{Enemy.Name}' tiene longitud cero.");
            fsm.TransitionTo("Idle");
            return;
        }

        movingForward = true;
        SetVisualDirection(movingForward);

        if (Enemy.animatedSprite != null)
        {
            Enemy.PlayAnimation("run");
        }
        else
        {
            GD.PushWarning($"Enemy '{Enemy.Name}' no tiene AnimatedSprite2D asignado en su script Enemy.");
        }
    }

    public override void Exit()
    {
        StopTimer();
        Enemy.Velocity = Vector2.Zero;
    }

    public override void PhysicsUpdate(float delta)
    {
        if (raycast.IsColliding() && raycast.GetCollider() is Player)
        {
            fsm.TransitionTo("Attack");
        }
        if (pathFollow == null || path == null || path.Curve == null)
        {
            return;
        }

        MoveOnPath(delta);
    }

    public void OnTimerTimeout()
    {
        if (fsm.currentState == this)
        {
            fsm.TransitionTo("Idle");
        }
    }

    private void MoveOnPath(float delta)
    {
        float pathLength = path.Curve.GetBakedLength();
        if (pathLength <= 0f)
        {
            return; // Evitar división por cero
        }

        float progressIncrement = speed / pathLength * delta;
        float directionMultiplier = movingForward ? 1f : -1f;
        pathFollow.ProgressRatio = Mathf.Clamp(pathFollow.ProgressRatio + directionMultiplier * progressIncrement, 0f, 1f);

        // El PathFollow2D marca la posición objetivo; el cuerpo se mueve hacia ella
        Vector2 directionToTarget = pathFollow.GlobalPosition - Enemy.GlobalPosition;

        // (la clase base Enemy es la única que llama a MoveAndSlide)
        Enemy.Velocity = directionToTarget.Length() > 1.0f
            ? directionToTarget.Normalized() * speed
            : Vector2.Zero;

        if (movingForward && pathFollow.ProgressRatio >= 1f - PATH_END_TOLERANCE)
        {
            movingForward = false;
            pathFollow.ProgressRatio = 1f; // Snap al final exacto
            SetVisualDirection(movingForward);
        }
        else if (!movingForward && pathFollow.ProgressRatio <= PATH_END_TOLERANCE)
        {
            movingForward = true;
            pathFollow.ProgressRatio = 0f; // Snap al inicio exacto
            SetVisualDirection(movingForward);
        }
    }

    /// Dirección visual del sprite. Cada enemigo puede sobreescribirla si su
    /// setup visual difiere (p. ej. PinkyStar escala el nodo padre).
    protected virtual void SetVisualDirection(bool isMovingForward)
    {
        if (Enemy.animatedSprite != null)
        {
            Enemy.animatedSprite.Scale = new Vector2(isMovingForward ? 1 : -1, 1);
        }
    }
}
