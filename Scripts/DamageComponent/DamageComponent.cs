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

	private void _on_body_entered(Node2D body)
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

	public void _on_area_exited(Area2D area)
	{
		if (area.IsInGroup("Water"))
		{
			if (GetParent().HasMethod("switchToWater"))
			{
				GetParent<Node2D>().CallDeferred("switchToWater");
				// El cooldown de agua solo existe en el jugador
				if (GetParent() is Player player && player._waterCooldown != null)
				{
					player._waterCooldown.Stop();
				}
			}
			else
			{
				GD.PushError("Parent does not have switchToWater method.");
			}
		}
	}
	public void _on_area_entered(Area2D area)
	{
		if (area.IsInGroup("Water"))
		{
			if (GetParent().HasMethod("switchToWater"))
			{
				GetParent<Node2D>().CallDeferred("switchToWater");
			}
			else
			{
				GD.PushError("Parent does not have switchToWater method.");
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