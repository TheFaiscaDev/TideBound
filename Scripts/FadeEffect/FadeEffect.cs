using Godot;
using System;

public partial class FadeEffect : ColorRect
{
    [Signal]
    public delegate void FadedToBlackEventHandler();

    [Export]
    public float DefaultFadeDuration = 0.5f;

    private Tween _fadeTween;

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        Color = new Color(Color.R, Color.G, Color.B, 0);
        this.Visible = true;

        // Registro en el UIManager (autoload) para que otros nodos no usen rutas absolutas
        GetNodeOrNull<UIManager>("/root/UIManager")?.RegisterFade(this);
    }

    public void FadeToBlack(float duration = -1)
    {
        float actualDuration = (duration > 0) ? duration : DefaultFadeDuration;

        if (_fadeTween != null && _fadeTween.IsRunning())
        {
            _fadeTween.Kill();
        }

        _fadeTween = CreateTween();

        _fadeTween.TweenMethod(
            Callable.From<float>(SetAlpha),
            Color.A,
            1.0f,
            actualDuration
        ).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);

        _fadeTween.Finished += OnFadeToBlackFinished;
    }

    public void FadeFromBlack(float duration = -1)
    {
        float actualDuration = (duration > 0) ? duration : DefaultFadeDuration;

        if (_fadeTween != null && _fadeTween.IsRunning())
        {
            _fadeTween.Kill();
        }

        _fadeTween = CreateTween();

        _fadeTween.TweenMethod(
            Callable.From<float>(SetAlpha),
            Color.A,
            0.0f,
            actualDuration
        ).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
    }

    private void OnFadeToBlackFinished()
    {
        if (_fadeTween != null)
        {
            _fadeTween.Finished -= OnFadeToBlackFinished;
        }

        EmitSignal(SignalName.FadedToBlack);
    }

    private void SetAlpha(float alpha)
    {
        Color = new Color(Color.R, Color.G, Color.B, alpha);
    }
}
