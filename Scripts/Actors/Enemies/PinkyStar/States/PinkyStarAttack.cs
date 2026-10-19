using Godot;
using System;

public partial class PinkyStarAttack : State
{
    [Export] public RayCast2D raycast;
    // El enemigo se obtiene del ControlledNode de la FSM (sin asignar en el inspector)
    private Enemy enemy => GetControlledNode<Enemy>();

    [Export] public float velocity;

    public override void Enter()
    {
        enemy.PlayAnimation("attack");
        GetChild<Timer>(0).Start(); // Start the timer for the attack animation

        velocity = velocity == 0 ? 10.0f : velocity;
    }
    public override void Exit()
    {
        // Paramos el timer para que no dispare transiciones fuera del estado
        GetNodeOrNull<Timer>("Timer")?.Stop();
    }

    public override void Update(float delta)
    {

    }
    public override void PhysicsUpdate(float delta)
    {
        if (enemy.player == null)
        {
            return;
        }
        Vector2 movementDirection = (enemy.player.GlobalPosition - enemy.GlobalPosition).Normalized();
        enemy.Velocity = new Vector2(movementDirection.X * velocity, enemy.Velocity.Y);
        // La clase base Enemy es la única que llama a MoveAndSlide
    }
    public override void HandleInput(InputEvent @event) { }

    public void OnTimerTimeout()
    {
        if (fsm.currentState == this)
        {
            fsm.TransitionTo("Idle");
        }
    }
}
