using Godot;
using System;

[Tool]
[GlobalClass]
public partial class DamageComponent : Area2D
{
	[Export] public double DamageAmount = 1;
	[Export] public float PushForce = 100.0f;
	[Export] public float PushDuration = 0.2f;
	[Signal] public delegate void TakeDamageEventHandler(float DamageAmount);
	private Marker2D _respawnMarker;

	// FadeEffect se obtiene del UIManager (autoload), sin rutas absolutas
	private FadeEffect Fade => GetNodeOrNull<UIManager>("/root/UIManager")?.Fade;

	public void OnBodyEntered(Node2D body)
	{
		if(body.IsInGroup("Spikes")){
			_respawnMarker = body.GetParent().GetNodeOrNull<Marker2D>("Marker2D");
			if (Fade != null && _respawnMarker != null)
			{
				Fade.FadedToBlack += OnFadedToBlack;
				Fade.FadeToBlack(); // Iniciar fundido
			}
			return;
		}
		if (body.IsInGroup("Enemy"))
		{
			if (body is Enemy enemy)
			{
				DamageAmount = enemy.damage;
			}
			// El padre de esta hurtbox es quien recibe el daño (type-safe)
			if (GetParent() is IDamageable target)
			{
				target.TakeDamage((int)DamageAmount);
				EmitSignal(SignalName.TakeDamage, DamageAmount);
				target.ApplyKnockback(body.GlobalPosition, PushForce, PushDuration);
			}
		}


	}

	public void OnAreaExited(Area2D area)
	{
		if (area.IsInGroup("Water"))
		{
			if (GetParent().HasMethod("SwitchToWater"))
			{
				GetParent<Node2D>().CallDeferred("SwitchToWater");
				// El cooldown de agua solo existe en el jugador
				if (GetParent() is Player player && player._waterCooldown != null)
				{
					player._waterCooldown.Stop();
				}
			}
			else
			{
				GD.PushError("Parent does not have SwitchToWater method.");
			}
		}
	}
	public void OnAreaEntered(Area2D area)
	{
		if (area.IsInGroup("Water"))
		{
			if (GetParent().HasMethod("SwitchToWater"))
			{
				GetParent<Node2D>().CallDeferred("SwitchToWater");
			}
			else
			{
				GD.PushError("Parent does not have SwitchToWater method.");
			}
		}
	}
	private void OnFadedToBlack()
	{
		// Reposicionar jugador al marker
		if (_respawnMarker != null)
		{
			GetParent<Node2D>().GlobalPosition = _respawnMarker.GlobalPosition;
		}

		// Desconectar señal y hacer fundido de regreso
		if (Fade != null)
		{
			Fade.FadedToBlack -= OnFadedToBlack;
			Fade.FadeFromBlack();
		}
	}
}