using Godot;
using System;

/// <summary>
/// Autoload que gestiona el ciclo de vida de los enemigos del nivel:
/// - Spawn: activa enemigos vivos cuando el jugador se acerca.
/// - Despawn lógico: desactiva enemigos vivos que están lejos (dejan de procesar física/FSM).
/// - Respawn: resetea enemigos muertos cuando el jugador está lo bastante lejos.
/// Las distancias se pueden ajustar desde el inspector en el autoload.
/// </summary>
public partial class RespawnManager : Node
{
    /// Distancia a la que un enemigo vivo se activa.
    [Export] public float ActivationDistance = 1800f;
    /// Histéresis para evitar parpadeos de activación en el límite.
    [Export] public float DeactivationBuffer = 250f;
    /// Distancia mínima del jugador para que un enemigo muerto reaparezca.
    [Export] public float RespawnDistance = 2400f;
    /// Intervalo entre comprobaciones (segundos).
    [Export] public double CheckInterval = 0.5;

    private double _elapsed;

    public override void _Process(double delta)
    {
        _elapsed += delta;
        if (_elapsed < CheckInterval)
        {
            return;
        }
        _elapsed = 0;

        if (GetTree().GetFirstNodeInGroup("Player") is not Node2D player || !GodotObject.IsInstanceValid(player))
        {
            return;
        }

        foreach (Node node in GetTree().GetNodesInGroup("Enemy"))
        {
            if (node is not Enemy enemy || !GodotObject.IsInstanceValid(enemy))
            {
                continue;
            }

            float distance = enemy.GlobalPosition.DistanceTo(player.GlobalPosition);

            if (enemy.IsDead)
            {
                // Respawn: solo cuando el jugador está lo bastante lejos
                if (distance > RespawnDistance)
                {
                    enemy.ResetEnemy();
                }
            }
            else if (distance > ActivationDistance + DeactivationBuffer)
            {
                enemy.SetActive(false); // Despawn lógico
            }
            else if (distance < ActivationDistance)
            {
                enemy.SetActive(true); // Spawn
            }
        }
    }
}
