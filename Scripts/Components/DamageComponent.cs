using Godot;
using System;

/// <summary>
/// Hurtbox genérica (Area2D): el padre es quien recibe el daño.
/// Responsabilidades actuales:
/// 1. Contacto con enemigos → daño + knockback al padre (type-safe vía IDamageable).
/// 2. Pinchos (grupo "Spikes") → fade y teletransporte al Marker2D de respawn.
///    NOTA DE DISEÑO: es intencionado que los pinchos NO quiten vida, solo reposicionan.
/// 3. Agua (grupo "Water") → notifica al jugador con EnterWater/ExitWater explícitos
///    (el daño inicial solo ocurre al entrar; ver Player.EnterWater).
/// Pendiente: separar pinchos y agua en scripts propios.
/// </summary>
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
		if (body.IsInGroup("Spikes"))
		{
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
			// El daño lo define el enemigo que toca; DamageAmount queda como fallback
			// (no se muta el export: era estado compartido confuso entre enemigos)
			double damage = body is Enemy enemy ? enemy.damage : DamageAmount;
			// El padre de esta hurtbox es quien recibe el daño (type-safe)
			if (GetParent() is IDamageable target)
			{
				target.TakeDamage((int)damage);
				EmitSignal(SignalName.TakeDamage, damage);
				target.ApplyKnockback(body.GlobalPosition, PushForce, PushDuration);
			}
		}
	}

	public void OnAreaEntered(Area2D area)
	{
		if (area.IsInGroup("Water"))
		{
			if (GetParent() is Player player)
			{
				player.CallDeferred(Player.MethodName.EnterWater);
			}
			else
			{
				GD.PushError("DamageComponent: el padre no es un Player; no puede entrar en agua.");
			}
		}
	}

	public void OnAreaExited(Area2D area)
	{
		if (area.IsInGroup("Water"))
		{
			if (GetParent() is Player player)
			{
				player.CallDeferred(Player.MethodName.ExitWater);
			}
			else
			{
				GD.PushError("DamageComponent: el padre no es un Player; no puede salir del agua.");
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
