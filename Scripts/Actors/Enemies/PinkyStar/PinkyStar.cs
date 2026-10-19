using Godot;
using System;

public partial class PinkyStar : Enemy
{
    private AnimationNodeStateMachinePlayback _animationMachine;

    public override void _Ready()
    {
        base._Ready();
        _animationMachine = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
        AddToGroup("Enemy");
        if (GetNodeOrNull<HealthComponent>("HealthComponent") == null)
        {
            GD.PushError("HealthComponent not found in PinkyStar node.");
        }
    }

    // PinkyStar anima con AnimationTree en vez de AnimatedSprite2D
    public override void PlayAnimation(string animationName)
    {
        _animationMachine?.Travel(animationName);
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
