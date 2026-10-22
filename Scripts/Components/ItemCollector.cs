using Godot;
using System;

public partial class ItemCollector : Area2D
{
    // La UI se obtiene del UIManager (autoload), sin rutas absolutas
    private UserInterface GetUI() => GetNodeOrNull<UIManager>("/root/UIManager")?.UI;

    public void OnBodyEntered(Node2D body)
    {
        UserInterface ui = GetUI();
        if (ui == null) return;
        GameState gameState = GetNodeOrNull<GameState>("/root/GameState");

        // Recolección type-safe: cada item sabe destruirse/abrirse a sí mismo.
        // Los items recogidos se marcan en GameState por su ID único (NodePath)
        // para que no reaparezcan al recargar la escena.
        switch (body)
        {
            case Key key when !ui.HasKey:
                gameState?.MarkItemCollected(key.GetPath());
                key.Destroy();
                ui.SetKey(true);
                break;
            case Chest chest when ui.HasKey:
                if (chest.Open())
                {
                    ui.SetKey(false);
                }
                break;
            case Coin coin:
                gameState?.MarkItemCollected(coin.GetPath());
                coin.Destroy();
                ui.AddCoins(1);
                break;
        }
    }
}
