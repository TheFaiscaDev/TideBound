using Godot;

/// <summary>
/// Checkpoint del mundo: al tocarlo, el jugador registra su posición como punto
/// de respawn en GameState (que además persiste la partida a disco) y restaura
/// su vida. La bandera cambia de color como feedback de activación.
/// </summary>
public partial class Checkpoint : Area2D
{
    private static readonly Color ActiveColor = new(0.45f, 1f, 0.55f);

    private bool _activated = false;
    private Polygon2D _flag;

    public override void _Ready()
    {
        _flag = GetNodeOrNull<Polygon2D>("Flag");
    }

    public void OnBodyEntered(Node2D body)
    {
        if (_activated || body is not Player player)
        {
            return;
        }
        _activated = true;

        string scenePath = GetTree().CurrentScene?.SceneFilePath ?? "";
        GetNodeOrNull<GameState>("/root/GameState")?.SetCheckpoint(scenePath, GlobalPosition);

        // El checkpoint restaura la vida del jugador
        player.GetNodeOrNull<HealthComponent>("HealthComponent")?.ResetHealth();

        // Feedback visual: la bandera se "enciende"
        if (_flag != null)
        {
            _flag.Color = ActiveColor;
        }
    }
}
