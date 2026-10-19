using Godot;
using System;

public partial class UserInterface : Control
{
    private TextureRect key;
    private Label coinsLabel;

    public int Coins { get; private set; } = 0;
    public bool HasKey { get; private set; } = false;

    public void AddCoins(int value)
    {
        Coins += value;
        coinsLabel.Text = Coins.ToString();
    }

    public void SetKey(bool value)
    {
        HasKey = value;
        key.Visible = value;
    }

    public override void _Ready()
    {
        key = GetNode<TextureRect>("Key");
        coinsLabel = GetNode<Label>("CoinsLabel");
        SetKey(false);

        // Registro en el UIManager (autoload) para que otros nodos no usen rutas absolutas
        if (GetNodeOrNull<UIManager>("/root/UIManager") is UIManager uiManager)
        {
            uiManager.RegisterUI(this);
            uiManager.RegisterHealthBar(GetNode<TextureProgressBar>("TextureProgressBar"));
        }
    }



}
