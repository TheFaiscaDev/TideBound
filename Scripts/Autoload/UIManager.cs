using Godot;

/// <summary>
/// Autoload que centraliza el acceso a los elementos de UI del nivel.
/// Los nodos de UI se registran en su _Ready y el resto los consulta aquí,
/// sin rutas absolutas tipo /root/Level1/...
/// </summary>
public partial class UIManager : Node
{
    public userInterface UI { get; private set; }
    public TextureProgressBar HealthBar { get; private set; }
    public FadeEffect Fade { get; private set; }

    public void RegisterUI(userInterface ui) => UI = ui;
    public void RegisterHealthBar(TextureProgressBar healthBar) => HealthBar = healthBar;
    public void RegisterFade(FadeEffect fade) => Fade = fade;

    public override void _Process(double delta)
    {
        // Limpia referencias liberadas al cambiar de escena
        if (UI != null && !GodotObject.IsInstanceValid(UI)) UI = null;
        if (HealthBar != null && !GodotObject.IsInstanceValid(HealthBar)) HealthBar = null;
        if (Fade != null && !GodotObject.IsInstanceValid(Fade)) Fade = null;
    }
}
