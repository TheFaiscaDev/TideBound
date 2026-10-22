using Godot;
using System;

public partial class Enemy : CharacterBody2D, IDamageable
{
	[Export] public float PushForce = 100.0f; // Fuerza base del retroceso que este enemigo aplica al jugador
	[Export] public float PushDuration = 0.1f; // Duración base del retroceso que aplica al jugador
	[Export] public double damage;
	[Export] public CollisionShape2D collisionShape;
	[Export] public AnimatedSprite2D animatedSprite;
	[Export] public AnimationTree animationTree;
	[Export] public float CorpseDuration = 2.0f; // Tiempo que el cadáver permanece visible antes del despawn

	// Referencia al jugador, resuelta automáticamente por grupo (sin asignar en el inspector)
	public Player player { get; private set; }

	private KnockbackComponent _knockback;
	private HealthComponent _healthComponent;

	// --- Estado de muerte / spawn / respawn ---
	protected bool _isDead = false;
	public bool IsDead => _isDead;

	private Vector2 _initialPosition;
	private float _initialProgressRatio;
	private Timer _corpseTimer;

	public override void _Ready()
	{
		player = GetTree().GetFirstNodeInGroup("Player") as Player;
		_knockback = GetNodeOrNull<KnockbackComponent>("KnockbackComponent");
		_healthComponent = GetNodeOrNull<HealthComponent>("HealthComponent");

		// Guardamos el estado inicial para poder respawnear al enemigo
		_initialPosition = Position;
		if (GetParent() is PathFollow2D pathFollow)
		{
			_initialProgressRatio = pathFollow.ProgressRatio;
		}

		_corpseTimer = new Timer { OneShot = true, WaitTime = CorpseDuration };
		AddChild(_corpseTimer);
		_corpseTimer.Timeout += OnCorpseExpired;
	}

	// --- IDamageable ---

	public void TakeDamage(int amount)
	{
		_healthComponent?.TakeDamage(amount);
	}

	public void ApplyKnockback(Vector2 sourcePosition, float force, float duration)
	{
		if (_isDead) return;
		_knockback?.Apply(sourcePosition, force, duration);
		OnKnockback();
	}

	/// Hook para que cada enemigo reaccione al retroceso (p. ej. cambiar al estado Hit).
	protected virtual void OnKnockback() { }

	/// Reproduce una animación del enemigo. Cada enemigo decide su sistema
	/// (AnimatedSprite2D por defecto; PinkyStar usa AnimationTree, BigRed mezcla con AnimationPlayer).
	public virtual void PlayAnimation(string animationName)
	{
		animatedSprite?.Play(animationName);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_isDead) return;

		if (_knockback != null && _knockback.TryComputeVelocity(delta, Velocity.Y, out Vector2 knockbackVelocity))
		{
			Velocity = knockbackVelocity;
		}

		if (!IsOnFloor())
		{
			// La gravedad es por segundo: hay que escalarla por delta
			Velocity += GetGravity() * (float)delta;
		}

		// Único MoveAndSlide del enemigo: los estados solo asignan Velocity
		MoveAndSlide();
	}

	// --- Muerte / despawn / respawn ---

	public void OnHealthComponentObjectDestroyed() => Die();

	protected virtual void Die()
	{
		if (_isDead) return;
		_isDead = true;
		_knockback?.Cancel();
		Velocity = Vector2.Zero;

		if (collisionShape != null)
		{
			collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		}

		SetHurtBoxActive(false);

		if (GetNodeOrNull<StateMachine>("FSM") is StateMachine fsm)
		{
			fsm.ProcessMode = ProcessModeEnum.Disabled;
		}

		OnDeathAnimation();
		_corpseTimer.Start();
	}

	/// Cada enemigo reproduce aquí su animación de muerte (una sola vez).
	protected virtual void OnDeathAnimation() { }

	private void OnCorpseExpired()
	{
		if (!_isDead) return;
		// Despawn lógico: el cadáver desaparece y el enemigo queda dormido
		Visible = false;
		SetActive(false);
	}

	/// Activa/desactiva el procesamiento del enemigo (usado por RespawnManager).
	public void SetActive(bool active)
	{
		ProcessMode = active ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
	}

	/// Restaura el enemigo a su estado inicial (usado por RespawnManager).
	public virtual void ResetEnemy()
	{
		_isDead = false;
		_knockback?.Cancel();
		Velocity = Vector2.Zero;

		Visible = true;
		SetActive(true);

		if (collisionShape != null)
		{
			collisionShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
		}

		SetHurtBoxActive(true);

		if (GetParent() is PathFollow2D pathFollow)
		{
			// Bajo un PathFollow2D la posición la impone el path: restaurar el
			// progreso basta (asignar Position además peleaba con el path)
			pathFollow.ProgressRatio = _initialProgressRatio;
		}
		else
		{
			Position = _initialPosition;
		}

		if (GetNodeOrNull<HealthComponent>("HealthComponent") is HealthComponent healthComponent)
		{
			healthComponent.ResetHealth();
		}

		if (GetNodeOrNull<StateMachine>("FSM") is StateMachine fsm)
		{
			fsm.ProcessMode = ProcessModeEnum.Inherit;
			fsm.ResetStateMachine();
		}
	}

	private void SetHurtBoxActive(bool active)
	{
		foreach (Node child in GetChildren())
		{
			if (child is DamageComponent hurtBox)
			{
				hurtBox.SetDeferred(Area2D.PropertyName.Monitoring, active);
				hurtBox.SetDeferred(Area2D.PropertyName.Monitorable, active);
			}
		}
	}
}
