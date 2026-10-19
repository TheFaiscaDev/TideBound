using Godot;
using System;

public partial class BigRed : Enemy
{
    public override void _Ready()
    {
        base._Ready();
        AddToGroup("Enemy");
        if (GetNodeOrNull<HealthComponent>("HealthComponent") == null)
        {
            GD.PushError("HealthComponent not found in BigRed node.");
        }
    }

    protected override void OnKnockback()
    {
        GetNode<StateMachine>("FSM").TransitionTo("Hit");
    }

    protected override void OnDeathAnimation()
    {
        if (animatedSprite != null && animatedSprite.GetNodeOrNull<AnimationPlayer>("AnimationPlayer") is AnimationPlayer animPlayer)
        {
            animPlayer.Play("dead");
        }
    }
}

