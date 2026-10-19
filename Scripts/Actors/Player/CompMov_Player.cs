using Godot;
using System;

public partial class CompMov_Player : Node2D
{
	private CharacterBody2D parent;
	[Export] public AnimationTree animationTree;
	[Export] public AnimatedSprite2D animatedSprite;

	private Vector2 direction;
	private float speed = 130f;
	private float jumpVelocity = -320f;
	private float fallMultiplier = 1.5f;
	private float lowJumpMultiplier = 1.7f;
	private float coyoteTime = 0.1f;
	private float jumpBufferTime = 0.1f;

	private float coyoteTimeCounter = 0f;
	private float jumpBufferCounter = 0f;
	private Vector2 velocity;

	private bool canDoubleJump = false;
	private IHook parenHook;


	public AnimationNodeStateMachinePlayback fsm;
	public override void _Ready()
	{
		parent = (CharacterBody2D)this.GetParent();
		animationTree.Active = true;
		fsm = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
		if(parent is IHook hook){
			parenHook = hook;
		}
	}
	public override void _PhysicsProcess(double delta)
	{
		Movement((float)delta);
		Attack();
		
	}

	private void Attack()
	{
		if (parent is IAttackable parentAttack)
		{

			if (Input.IsActionPressed("attack") && !parentAttack.IsAttacking)
			{
				parentAttack.IsAttacking = true;
				fsm.Travel("attack1");
			}
			if (parent.IsOnCeiling() && velocity.Y < 0)
			{
				velocity.Y = 0;
			}
		}
	}
	public void SkipGravityNextFrame()
	{
		parenHook.SkipGravityFrame = true;
	}

	private void Movement(float delta)
	{
		if (parenHook.SkipGravityFrame)
		{
			parenHook.SkipGravityFrame = false;
			velocity.Y = parent.Velocity.Y;
			return; // Nos saltamos el frame para evitar caída rápida
		}
		if (parent is IHook parentHook)
		{
			if (parentHook.IsHooked)
			{
				// Cancelar movimiento normal
				return;
			}
		}

		direction = new Vector2(Input.GetActionStrength("right") - Input.GetActionStrength("left"), 0);
		if (!parent.IsOnFloor())
		{
			if (direction.X != 0)
			{
				velocity.X = Mathf.Lerp(parent.Velocity.X, direction.X * speed, 8f * delta); // control aéreo
			}
			else
			{
				velocity.X = Mathf.Lerp(parent.Velocity.X, 0, 1f * delta); // frenado aéreo suave
			}
			if (velocity.Y > 0) // Cayendo
			{
				// 🔹 Lerp para suavizar la caída
				velocity.Y = Mathf.Lerp(velocity.Y, velocity.Y + parent.GetGravity().Y * fallMultiplier, 0.75f * delta);
				if (fsm.GetCurrentNode() != "fall")
				{
					fsm.Travel("fall");
				}
			}
			else if (!Input.IsActionPressed("jump")) // Salto cortado
			{
				velocity.Y = Mathf.Lerp(velocity.Y, velocity.Y + parent.GetGravity().Y * lowJumpMultiplier, 1.7f * delta);
			}
			else // Salto normal
			{
				velocity.Y += parent.GetGravity().Y * delta;
			}
		}
		else
		{
			velocity.X = Mathf.Lerp(parent.Velocity.X, direction.X * speed, 14f * delta);
			velocity.Y = 0;
			canDoubleJump = true;
			if (parent is IAttackable parentAttack)
			{
				if (direction.X != 0 && !parentAttack.IsAttacking)
				{

					fsm.Travel("run");
				}
				else
				{
					fsm.Travel("idle");
					parentAttack.IsAttacking = false;
				}
			}
		}
		if (parent.IsOnFloor())
			coyoteTimeCounter = coyoteTime;
		else
			coyoteTimeCounter -= delta;

		if (Input.IsActionJustPressed("jump"))
			jumpBufferCounter = jumpBufferTime;
		else
			jumpBufferCounter -= delta;

		// **Salto**
		if (jumpBufferCounter > 0)
		{
			if (coyoteTimeCounter > 0 || canDoubleJump)
			{
				// isHooked = false; TODO:Ver esta mierda
				velocity.Y = Mathf.Lerp(parent.Velocity.Y, jumpVelocity, 65f * delta);
				jumpBufferCounter = 0;
				coyoteTimeCounter = 0;

				if (!parent.IsOnFloor() && canDoubleJump)
				{
					canDoubleJump = false; // Se usó el doble salto
					fsm.Travel("jump"); // Puedes poner una animación especial si quieres
				}
				else
				{
					fsm.Travel("jump");
				}
			}
		}

		if (direction.X != 0)
			animatedSprite.Scale = new Vector2(Mathf.Sign(direction.X), 1);

		parent.Velocity = velocity;
	}
	
}
