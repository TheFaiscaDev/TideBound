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
	public Timer _waterCooldown;

	private bool died = false;
	private TextureProgressBar progressBar;


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
		// La UI se registra en el UIManager después del _Ready del jugador
		CallDeferred(MethodName.InitHealthBar);
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
	public void _on_sword_hit_area_entered(Area2D area)
	{
		// El DamageComponent es la hurtbox: el objetivo real es su padre
		if (area is DamageComponent && area.GetParent() is IDamageable target)
		{
			target.ApplyKnockback(GlobalPosition, PushForce, PushDuration);
			target.TakeDamage(1);
		}
	}
	public void _on_sword_hit_body_entered(Node2D node)
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

	private bool _isInWater = false;

	public void ApplyToxicEffect()
	{

		_healthComponent.TakeDamage(1);
		fsm.Travel("hit");
		if (_isInWater)
		{
			_waterCooldown.Start();
		}
		else
		{
			_waterCooldown.Stop();
		}
	}

	public void switchToWater()
	{
		_isInWater = !_isInWater;
		ApplyToxicEffect();
	}

	public void _on_water_cooldown_timeout()
	{
		ApplyToxicEffect();
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

	public void _on_health_component_object_destroyed()
	{
		if (died) return;

		GetNode("MovementComponent").QueueFree();
		GetNode("HookComponent").QueueFree();
		GetNode("HurtBox").QueueFree();
		this.SetCollisionMaskValue(3, false);
		died = true;
		fsm.Travel("dead");

		// Tras la animación de muerte, recargamos la escena (respawn del jugador)
		GetTree().CreateTimer(2.0).Timeout += () =>
		{
			if (GodotObject.IsInstanceValid(this))
			{
				GetTree().ReloadCurrentScene();
			}
		};
	}
	public void _on_health_component_health_changed(int newHealth)
	{
		if (progressBar != null)
		{
			progressBar.Value = newHealth;
		}
	}


}











