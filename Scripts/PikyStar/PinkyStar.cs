using Godot;
using System;

public partial class PinkyStar : Enemy
{
    public AnimationNodeStateMachinePlayback animationMachine;

    public override void _Ready()
    {
        base._Ready();
        animationMachine = (AnimationNodeStateMachinePlayback)animationTree.Get("parameters/playback");
        AddToGroup("Enemy");
        if (GetNodeOrNull<HealthComponent>("HealthComponent") == null)
        {
            GD.PushError("HealthComponent not found in PikyStar node.");
        }
    }

    protected override void OnKnockback()
    {
        GetNode<StateMachine>("FSM").TransitionTo("Hit");
    }

    protected override void OnDeathAnimation()
    {
        animationMachine.Travel("dead");
    }
}
