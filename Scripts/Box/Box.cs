using Godot;
using System;


public partial class Box : RigidBody2D, IDamageable
{
	[Export] AnimationPlayer player;
	[Export] public int Health { get; set; } = 1;

	private bool destroyed = false;

	// --- IDamageable ---

	public void TakeDamage(int amount)
	{
		if (destroyed) return;
		Health -= amount;
		if (Health <= 0)
		{
			destroyed = true;
			player.Play("destroyed");
		}
	}

	public void ApplyKnockback(Vector2 sourcePosition, float force, float duration)
	{
		// RigidBody2D: aplicamos un impulso en vez de velocidad directa
		Vector2 direction = (GlobalPosition - sourcePosition).Normalized();
		ApplyImpulse(direction * force);
	}

	public void _on_animation_player_animation_finished(string anim_name){
		if (anim_name == "destroyed")
		{
			this.QueueFree();
		}
	}
}
