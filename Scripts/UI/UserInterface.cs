using Godot;
using System;

/// <summary>
/// HUD del nivel: monedas y llave. Es una VISTA PURA: el estado vive en el
/// autoload GameState (persiste entre escenas y se guarda en disco); esta clase
/// solo refleja sus cambios vía señales y le delega las escrituras.
/// </summary>
public partial class UserInterface : Control
{
    private TextureRect key;
    private Label coinsLabel;
    private GameState _gameState;

    public int Coins { get; private set; } = 0;
    public bool HasKey { get; private set; } = false;

    public override void _Ready()
    {
        key = GetNode<TextureRect>("Key");
        coinsLabel = GetNode<Label>("CoinsLabel");

        // Registro en el UIManager (autoload) para que otros nodos no usen rutas absolutas
        if (GetNodeOrNull<UIManager>("/root/UIManager") is UIManager uiManager)
        {
            uiManager.RegisterUI(this);
            uiManager.RegisterHealthBar(GetNode<TextureProgressBar>("TextureProgressBar"));
        }

        // Enlace con GameState: la UI solo refleja su estado
        _gameState = GetNodeOrNull<GameState>("/root/GameState");
        if (_gameState != null)
        {
            _gameState.CoinsChanged += OnGameStateCoinsChanged;
            _gameState.KeyChanged += OnGameStateKeyChanged;
            OnGameStateCoinsChanged(_gameState.Coins);
            OnGameStateKeyChanged(_gameState.HasKey);
        }
        else
        {
            SetKey(false);
        }
    }

    public override void _ExitTree()
    {
        if (_gameState != null)
        {
            _gameState.CoinsChanged -= OnGameStateCoinsChanged;
            _gameState.KeyChanged -= OnGameStateKeyChanged;
        }
    }

    public void AddCoins(int value)
    {
        if (_gameState != null)
        {
            _gameState.AddCoins(value);
        }
        else
        {
            OnGameStateCoinsChanged(Coins + value);
        }
    }

    public void SetKey(bool value)
    {
        if (_gameState != null)
        {
            _gameState.SetKey(value);
        }
        else
        {
            OnGameStateKeyChanged(value);
        }
    }

    private void OnGameStateCoinsChanged(int coins)
    {
        Coins = coins;
        coinsLabel.Text = Coins.ToString();
    }

    private void OnGameStateKeyChanged(bool hasKey)
    {
        HasKey = hasKey;
        key.Visible = hasKey;
    }
}
