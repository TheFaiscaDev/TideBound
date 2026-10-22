using Godot;
using System;

public partial class Coin : RigidBody2D
{
    private AnimatedSprite2D animatedSprite;
    private AnimationPlayer animationPlayer;
    public override void _Ready()
    {
        // Si la moneda ya se recogió en esta partida, no reaparece (persistencia GameState)
        if (GetNodeOrNull<GameState>("/root/GameState") is GameState gameState
            && gameState.IsItemCollected(GetPath()))
        {
            QueueFree();
            return;
        }

        animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public void Destroy()
    {
        animationPlayer.Play("destroy");
    }

}
