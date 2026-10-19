using Godot;
using System;

public partial class Coin : RigidBody2D
{
    private AnimatedSprite2D animatedSprite;
    private AnimationPlayer animationPlayer;
    public override void _Ready()
    {
        animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public void Destroy()
    {
        animationPlayer.Play("destroy");
    }

}
