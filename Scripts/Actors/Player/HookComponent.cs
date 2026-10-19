using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class HookComponent : Node2D
{
	[Export] public RayCast2D raycast;
	private float hookStopDistance = 50f;
	private float hookSpeed = 300f;
	private CharacterBody2D parent;
	private Vector2 velocity;

	private Rope line;
	private StaticBody2D cursor;

	private IHook parentHook;
	private float springStrength = 5f;  // Constante de rigidez (Hooke)
	private float damping = 0.5f;
	[Export] public AnimatedSprite2D animatedSprite;
	private MouseWithJoystick mouse;


	public override void _Ready()
	{
		parent = (CharacterBody2D)this.GetParent();
		if (parent is IHook hook)
		{
			this.parentHook = hook;
		}
		line = parentHook.Line;
		cursor = parentHook.Cursor;
		mouse = GetNode<MouseWithJoystick>("/root/MouseWithJoystick");
	}

	public override void _Input(InputEvent @event)
	{
		if (Input.IsActionJustPressed("hook"))
		{
			velocity = new Vector2(parent.Velocity.X, 0);
			if (raycast.IsColliding())
			{
				line.ClearPoints();
				cursor.Position = raycast.GetCollisionPoint();
				float distance = this.Position.DistanceTo(cursor.Position);
				line.start = cursor.GlobalPosition;
				parentHook.IsHooked = true;
			}
			
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		HandleHook((float)delta);
	}

	public void HandleHook(float delta)
	{
		raycast.LookAt(mouse.VirtualCursorPosition);
		// Tras LookAt el eje +X apunta al cursor; el rayo usa +Y local
		// (target_position = (0, 244)), así que giramos 90° para alinearlo.
		raycast.Rotation += Mathf.Pi / 2f;
		if (!parentHook.IsHooked || !Input.IsActionPressed("hook"))
		{
			line.Visible = false;
			line.ClearPoints();
			parentHook.IsHooked = false;
			return;
		}

		parentHook.SkipGravityFrame = true;
		line.Visible = true;
		line.end = this.GlobalPosition;

		Vector2 direction = (cursor.GlobalPosition) - parent.GlobalPosition;
		Vector2 force = new Vector2(direction.X * springStrength - parent.Velocity.X * damping, (direction.Y + hookStopDistance) * springStrength - parent.Velocity.Y * damping);
		velocity += force * delta;


		velocity = velocity.LimitLength(300f);

		parent.Velocity = velocity;
		if (velocity.Normalized().X != 0)
			animatedSprite.Scale = new Vector2(Mathf.Sign(velocity.Normalized().X), 1);

	}
}

