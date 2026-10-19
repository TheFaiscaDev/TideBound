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

        // Recolección type-safe: cada item sabe destruirse/abrirse a sí mismo
        switch (body)
        {
            case Key key when !ui.HasKey:
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
                coin.Destroy();
                ui.AddCoins(1);
                break;
        }
    }
}
