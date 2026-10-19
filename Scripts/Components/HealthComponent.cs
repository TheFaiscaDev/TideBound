using Godot;
using System;

[GlobalClass]
public partial class HealthComponent : Node
{
    [Export] public int Health { get; set; } = 100;

    /// Segundos de invulnerabilidad tras recibir daño (0 = sin i-frames).
    [Export] public float InvulnerabilityDuration = 0.5f;

    [Signal] public delegate void HealthChangedEventHandler(int newHealth);
    [Signal] public delegate void ObjectDestroyedEventHandler();

    private int _maxHealth;
    private float _invulnerabilityTimer = 0f;

    public override void _Ready()
    {
        _maxHealth = Health;
    }

    public override void _Process(double delta)
    {
        if (_invulnerabilityTimer > 0f)
        {
            _invulnerabilityTimer -= (float)delta;
        }
    }

    public void TakeDamage(int amount)
    {
        // Ignorar daño durante los i-frames o si ya está destruido
        if (_invulnerabilityTimer > 0f || Health <= 0)
        {
            return;
        }

        Health = Mathf.Max(Health - amount, 0);
        _invulnerabilityTimer = InvulnerabilityDuration;
        EmitSignal(SignalName.HealthChanged, Health);

        if (Health <= 0)
        {
            EmitSignal(SignalName.ObjectDestroyed);
        }
    }

    /// Restaura la vida al valor inicial (usado al respawnear).
    public void ResetHealth()
    {
        Health = _maxHealth;
        _invulnerabilityTimer = 0f;
        EmitSignal(SignalName.HealthChanged, Health);
    }
}