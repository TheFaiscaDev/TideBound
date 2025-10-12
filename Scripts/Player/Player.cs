using System;
using Godot;


public partial class Player : CharacterBody2D, AttackInterface, IHook
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

	[Export] public float PushForce = 200.0f; // Fuerza del tirón hacia atrás
	[Export] public float PushDuration = 0.2f; // Duración del tirón en segundos

	private bool _isBeingPushed = false;
	private Vector2 _pushDirection = Vector2.Zero;
	private float _pushTimer = 0.0f;
	private Vector2 _velocity = Vector2.Zero; // Mantén un registro de la velocidad normal del jugador
	[Export] public AnimationTree animationTree;
	public AnimationNodeStateMachinePlayback fsm;
	private HealthComponent _healthComponent;
	private float _currentPushForce = 0.0f;
	private float _currentPushDuration = 0.0f;
	public Timer _waterCooldown;

	private bool died = false;
	private TextureProgressBar progressBar;


	public override void _Ready()
	{

		progressBar = GetNode<TextureProgressBar>("/root/Level1/Menu/UI/TextureProgressBar");
		fsm = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
		_healthComponent = GetNodeOrNull<HealthComponent>("HealthComponent");
		if (_healthComponent == null)
		{
			GD.PushError("HealthComponent not found in Player node.");
		}
		_waterCooldown = GetNodeOrNull<Timer>("WaterCooldown");
	}

	/**
	 * Called when the player's sword trigger collides with an object.
	 * @param area The area that the sword trigger collided with.
	 * This is usually a DamageComponent that will take damage.
	 */
	public void _on_sword_hit_area_entered(Area2D area)
	{
		if (area is DamageComponent)
		{
			DamageComponent hurt = (DamageComponent)area;
			hurt.GetParent().CallDeferred("ApplyPushback", GlobalPosition, PushForce, PushDuration);
			if (hurt.GetParent().HasNode("HealthComponent"))
			{
				HealthComponent targetHealth = hurt.GetParent().GetNode<HealthComponent>("HealthComponent");
				if (targetHealth != null)
				{
					targetHealth.TakeDamage(1);
				}
				else
				{
					GD.PushError("HealthComponent not found in enemy node.");
				}
			}
			else
			{
				GD.PushError("Enemy does not have a HealthComponent.");
			}
		}
	}
	public void _on_sword_hit_body_entered(Node2D node)
	{

		if (node.IsInGroup("Enemy"))
		{
			if (node.HasMethod("ApplyPushback"))
			{
				GD.Print("Player: Sword hit enemy, applying pushback.");
				node.CallDeferred("ApplyPushback", GlobalPosition, PushForce, PushDuration);
			}
			else
			{
				GD.PushError("Enemy does not have ApplyPushback method.");
			}
			if (node.HasNode("HealthComponent"))
			{
				HealthComponent targetHealth = node.GetNode<HealthComponent>("HealthComponent");
				if (targetHealth != null)
				{
					targetHealth.TakeDamage(1);
				}
				else
				{
					GD.PushError("HealthComponent not found in enemy node.");
					GD.Print("Player: Enemy does not have a HealthComponent.");
				}
			}
			else
			{
				GD.PushError("Enemy does not have a HealthComponent.");
			}
		}

	}


	public override void _PhysicsProcess(double delta)
	{
		if (died)
		{
			fsm.Travel("dead");
			return;
		}

		if (_isBeingPushed)
		{
			_velocity = new Vector2(_pushDirection.X * PushForce * 3, Mathf.Lerp(Velocity.Y, _pushDirection.Y * PushForce, 90f * (float)delta));

			_pushTimer += (float)delta;
			if (_pushTimer >= PushDuration) // Usa la duración base del jugador aquí
			{
				_isBeingPushed = false;
				_pushTimer = 0.0f;
				_velocity = Vector2.Zero;
			}

			Velocity = _velocity;
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
	public void ApplyPushback(Vector2 sourcePosition, float force, float duration)
	{
		_pushDirection = (GlobalPosition - sourcePosition).Normalized();
		PushForce = force; // Sobreescribe la fuerza base con la del impacto
		PushDuration = duration; // Sobreescribe la duración base con la del impacto
		_isBeingPushed = true;
		_pushTimer = 0.0f;
		fsm.Travel("hit");
	}

	public void _on_health_component_object_destroyed()
	{
		GetNode("MovementComponent").QueueFree();
		GetNode("HookComponent").QueueFree();
		GetNode("HurtBox").QueueFree();
		this.SetCollisionMaskValue(3, false);
		died = true;

	}
	public void _on_health_component_health_changed(int newHealth)
	{
		GD.Print("Player: Health changed to " + newHealth);
		progressBar.Value = newHealth;
	}


}











