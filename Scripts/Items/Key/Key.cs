using Godot;
using System;

public partial class Key : RigidBody2D
{
    private AnimatedSprite2D animatedSprite;
    private AnimationPlayer player;
    private bool _destroying = false;

    public override void _Ready()
    {
        // Si la llave ya se recogió en esta partida, no reaparece (persistencia GameState)
        if (GetNodeOrNull<GameState>("/root/GameState") is GameState gameState
            && gameState.IsItemCollected(GetPath()))
        {
            QueueFree();
            return;
        }

        animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        player = animatedSprite?.GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
        if (animatedSprite == null || player == null)
        {
            GD.PushError($"Key '{Name}': falta el nodo AnimatedSprite2D o su AnimationPlayer hijo.");
            return;
        }
        player.Play("idle");
    }

    public void OnBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            player?.Stop();
        }
    }

    public void Destroy()
    {
        if (_destroying)
        {
            return;
        }
        _destroying = true;
        // Detiene la física y reproduce la animación de recogida (con partículas);
        // el nodo se libera en OnAnimationFinished (señal conectada en key.tscn)
        Freeze = true;
        player?.Play("destroy");
    }

    public void OnAnimationFinished(StringName animName)
    {
        if (animName == "destroy")
        {
            QueueFree();
        }
    }
}
