using System;
using Godot;


public partial class Player : CharacterBody2D, IAttackable, IHook, IDamageable
{
	private bool isAttacking = false;
	public bool IsAttacking { get => isAttacking; set => isAttacking = value; }
	private Rope line;
	[Export] public Rope Line { get => line; set => line = value; }
	private StaticBody2D cursor;
	[Export] public StaticBody2D Cursor { get => cursor; set => cursor = value; }
	private bool isHooked = false;
	public bool IsHooked { get => isHooked; set => isHooked = value; }
	private bool skipGravityFrame = false;
	public bool SkipGravityFrame { get => skipGravityFrame; set => skipGravityFrame = value; }

	[Export] public float PushForce = 200.0f; // Fuerza de retroceso que la espada aplica a los enemigos
	[Export] public float PushDuration = 0.2f; // Duración del retroceso aplicado a los enemigos

	[Export] public AnimationTree animationTree;
	public AnimationNodeStateMachinePlayback fsm;
	private HealthComponent _healthComponent;
	private KnockbackComponent _knockback;
	private Timer _waterCooldown;

	private bool died = false;
	private TextureProgressBar progressBar;
	private Vector2 _initialPosition;


	public override void _Ready()
	{
		fsm = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
		_healthComponent = GetNodeOrNull<HealthComponent>("HealthComponent");
		if (_healthComponent == null)
		{
			GD.PushError("HealthComponent not found in Player node.");
		}
		_knockback = GetNodeOrNull<KnockbackComponent>("KnockbackComponent");
		_waterCooldown = GetNodeOrNull<Timer>("WaterCooldown");
		_initialPosition = GlobalPosition;
		// La UI se registra en el UIManager después del _Ready del jugador
		CallDeferred(MethodName.InitHealthBar);
		// Si hay un checkpoint guardado en ESTA escena, el jugador aparece en él
		CallDeferred(MethodName.ApplySpawnPosition);
	}

	private void ApplySpawnPosition()
	{
		if (GetNodeOrNull<GameState>("/root/GameState") is GameState gameState)
		{
			GlobalPosition = gameState.GetRespawnPosition(GlobalPosition);
		}
	}

	private void InitHealthBar()
	{
		progressBar = GetNodeOrNull<UIManager>("/root/UIManager")?.HealthBar;
		if (progressBar != null && _healthComponent != null)
		{
			progressBar.Value = _healthComponent.Health;
		}
	}

	/**
	 * Called when the player's sword trigger collides with an object.
	 * @param area The area that the sword trigger collided with.
	 * This is usually a DamageComponent that will take damage.
	 */
	public void OnSwordHitAreaEntered(Area2D area)
	{
		// El DamageComponent es la hurtbox: el objetivo real es su padre
		if (area is DamageComponent && area.GetParent() is IDamageable target)
		{
			target.ApplyKnockback(GlobalPosition, PushForce, PushDuration);
			target.TakeDamage(1);
		}
	}
	public void OnSwordHitBodyEntered(Node2D node)
	{
		if (node.IsInGroup("Enemy") && node is IDamageable target)
		{
			target.ApplyKnockback(GlobalPosition, PushForce, PushDuration);
			target.TakeDamage(1);
		}
	}


	public override void _PhysicsProcess(double delta)
	{
		if (died)
		{
			return;
		}

		if (_knockback != null && _knockback.TryComputeVelocity(delta, Velocity.Y, out Vector2 knockbackVelocity))
		{
			Velocity = knockbackVelocity;
		}

		MoveAndSlide();
	}

	// Contador de masas de agua que solapan al jugador (soporta aguas adyacentes:
	// entrar en A, entrar en B y salir de A NO te saca del agua si sigues en B)
	private int _waterAreaCount = 0;
	public bool IsInWater => _waterAreaCount > 0;

	public void ApplyToxicEffect()
	{
		_healthComponent?.TakeDamage(1);
		fsm?.Travel("hit");
	}

	/// Llamado por DamageComponent al ENTRAR en una zona de agua (grupo "Water").
	/// El daño inicial solo se aplica al pasar de 0 a 1 áreas (nunca al salir).
	public void EnterWater()
	{
		_waterAreaCount++;
		if (_waterAreaCount == 1)
		{
			ApplyToxicEffect();
			_waterCooldown?.Start(); // daño periódico mientras IsInWater
		}
	}

	/// Llamado por DamageComponent al SALIR de una zona de agua (grupo "Water").
	public void ExitWater()
	{
		_waterAreaCount = Mathf.Max(_waterAreaCount - 1, 0);
		if (_waterAreaCount == 0)
		{
			_waterCooldown?.Stop();
		}
	}

	public void OnWaterCooldownTimeout()
	{
		if (IsInWater)
		{
			ApplyToxicEffect();
		}
	}
	// --- IDamageable ---

	public void TakeDamage(int amount)
	{
		_healthComponent?.TakeDamage(amount);
	}

	public void ApplyKnockback(Vector2 sourcePosition, float force, float duration)
	{
		if (died) return;
		_knockback?.Apply(sourcePosition, force, duration);
		fsm.Travel("hit");
	}

	public void OnHealthComponentObjectDestroyed()
	{
		if (died) return;

		// Los componentes se DESACTIVAN (no se liberan) para poder respawnear sin recargar la escena
		SetComponentsActive(false);
		this.SetCollisionMaskValue(3, false);
		died = true;
		fsm.Travel("dead");

		// Tras la animación de muerte: fade y respawn en el último checkpoint
		GetTree().CreateTimer(2.0).Timeout += () =>
		{
			if (GodotObject.IsInstanceValid(this))
			{
				RespawnAtCheckpoint();
			}
		};
	}

	private void SetComponentsActive(bool active)
	{
		foreach (string nodeName in new[] { "MovementComponent", "HookComponent", "HurtBox" })
		{
			Node node = GetNodeOrNull(nodeName);
			if (node == null)
			{
				GD.PushError($"Player: no se encontró el nodo hijo '{nodeName}'.");
				continue;
			}
			node.ProcessMode = active ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
			if (node is Area2D area)
			{
				area.SetDeferred(Area2D.PropertyName.Monitoring, active);
				area.SetDeferred(Area2D.PropertyName.Monitorable, active);
			}
		}
	}

	private void RespawnAtCheckpoint()
	{
		FadeEffect fade = GetNodeOrNull<UIManager>("/root/UIManager")?.Fade;
		if (fade != null)
		{
			fade.FadedToBlack += OnDeathFadedToBlack;
			fade.FadeToBlack();
		}
		else
		{
			OnDeathFadedToBlack();
		}
	}

	private void OnDeathFadedToBlack()
	{
		FadeEffect fade = GetNodeOrNull<UIManager>("/root/UIManager")?.Fade;
		if (fade != null)
		{
			fade.FadedToBlack -= OnDeathFadedToBlack;
		}

		// Respawn en el último checkpoint de esta escena (o en el spawn inicial del nivel)
		Vector2 respawnPosition = _initialPosition;
		if (GetNodeOrNull<GameState>("/root/GameState") is GameState gameState)
		{
			respawnPosition = gameState.GetRespawnPosition(_initialPosition);
		}

		GlobalPosition = respawnPosition;
		Velocity = Vector2.Zero;
		_healthComponent?.ResetHealth();
		_waterAreaCount = 0;
		_waterCooldown?.Stop();
		died = false;
		SetComponentsActive(true);
		this.SetCollisionMaskValue(3, true);
		fsm.Travel("idle");
		fade?.FadeFromBlack();
	}
	public void OnHealthComponentHealthChanged(int newHealth)
	{
		if (progressBar != null)
		{
			progressBar.Value = newHealth;
		}
	}


}











