using Godot;
using System;

public partial class ItemCollector : Area2D
{
    // La UI se obtiene del UIManager (autoload), sin rutas absolutas
    private userInterface GetUI() => GetNodeOrNull<UIManager>("/root/UIManager")?.UI;

    public void _on_body_entered(Node2D body)
    {
        userInterface ui = GetUI();
        if (ui == null) return;
        if (body.IsInGroup("Key"))
        {
            if (!ui.getKey())
            {
                if (body.HasMethod("destroy"))
                {
                    body.CallDeferred("destroy");
                }

                ui.setKey(true);
            }
        }
        if (body.IsInGroup("Chest"))
        {
            if (ui.getKey())
            {
                if (body.HasMethod("open"))
                {
                    ui.setKey(((Chest)body).open());
                }

            }
        }
        if (body.IsInGroup("Coin"))
        {
            body.CallDeferred("destroy");
            ui.addCoins(1);
        }
    }
}
