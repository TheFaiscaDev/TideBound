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

    // BigRed usa AnimatedSprite2D para la mayoría de animaciones y AnimationPlayer para "dead"
    public override void PlayAnimation(string animationName)
    {
        if (animationName == "dead" && animatedSprite != null
            && animatedSprite.GetNodeOrNull<AnimationPlayer>("AnimationPlayer") is AnimationPlayer animPlayer)
        {
            animPlayer.Play("dead");
            return;
        }
        base.PlayAnimation(animationName);
    }

    protected override void OnKnockback()
    {
        GetNode<StateMachine>("FSM").TransitionTo("Hit");
    }

    protected override void OnDeathAnimation()
    {
        PlayAnimation("dead");
    }
}

