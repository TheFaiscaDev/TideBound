using Godot;
using Godot.Collections;

/// <summary>
/// Autoload con el estado persistente del juego (patrón documentado en
/// docs/GUIA_ARQUITECTURA.md): monedas, llave(s), habilidades desbloqueadas,
/// items recogidos (por ID único de nodo, para que no reaparezcan) y checkpoints.
///
/// Se serializa a JSON en user://savegame.json: se guarda al activar un checkpoint
/// y al cerrar el juego, y se carga automáticamente al arrancar.
///
/// La UI lee/escribe contra GameState (vista pura); los niveles solo consultan.
/// </summary>
public partial class GameState : Node
{
    private const string SavePath = "user://savegame.json";

    [Signal] public delegate void CoinsChangedEventHandler(int coins);
    [Signal] public delegate void KeyChangedEventHandler(bool hasKey);
    [Signal] public delegate void CheckpointActivatedEventHandler();

    public int Coins { get; private set; } = 0;
    public bool HasKey { get; private set; } = false;

    /// Habilidades desbloqueadas (p. ej. "hook", "double_jump").
    private readonly System.Collections.Generic.HashSet<string> _abilities = new();
    /// IDs únicos (NodePath en la escena) de items ya recogidos: no reaparecen al recargar.
    private readonly System.Collections.Generic.HashSet<string> _collectedItems = new();

    /// Escena (ruta res://) donde está el último checkpoint activado.
    public string CheckpointScene { get; private set; } = "";
    public Vector2 CheckpointPosition { get; private set; } = Vector2.Zero;
    public bool HasCheckpoint => CheckpointScene != "";

    public override void _Ready()
    {
        LoadGame();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            SaveGame();
        }
    }

    // --- Monedas / llave ---

    public void AddCoins(int amount)
    {
        Coins += amount;
        EmitSignal(SignalName.CoinsChanged, Coins);
    }

    public void SetKey(bool value)
    {
        HasKey = value;
        EmitSignal(SignalName.KeyChanged, value);
    }

    // --- Habilidades ---

    public void UnlockAbility(string ability) => _abilities.Add(ability);
    public bool HasAbility(string ability) => _abilities.Contains(ability);

    // --- Items recogidos ---

    public void MarkItemCollected(string itemId) => _collectedItems.Add(itemId);
    public bool IsItemCollected(string itemId) => _collectedItems.Contains(itemId);

    // --- Checkpoints ---

    /// Registra un checkpoint (posición y escena) y guarda la partida en disco.
    public void SetCheckpoint(string scenePath, Vector2 position)
    {
        CheckpointScene = scenePath;
        CheckpointPosition = position;
        EmitSignal(SignalName.CheckpointActivated);
        SaveGame();
    }

    /// Posición de respawn para la escena actual: el checkpoint si pertenece a
    /// esta escena; si no, la posición dada como fallback (spawn del nivel).
    public Vector2 GetRespawnPosition(Vector2 fallback)
    {
        string currentScene = GetTree().CurrentScene?.SceneFilePath ?? "";
        return HasCheckpoint && CheckpointScene == currentScene ? CheckpointPosition : fallback;
    }

    // --- Persistencia en disco ---

    public void SaveGame()
    {
        var abilitiesArray = new Array();
        foreach (string ability in _abilities)
        {
            abilitiesArray.Add(ability);
        }
        var collectedArray = new Array();
        foreach (string itemId in _collectedItems)
        {
            collectedArray.Add(itemId);
        }

        var data = new Dictionary
        {
            ["coins"] = Coins,
            ["has_key"] = HasKey,
            ["abilities"] = abilitiesArray,
            ["collected_items"] = collectedArray,
            ["checkpoint_scene"] = CheckpointScene,
            ["checkpoint_x"] = CheckpointPosition.X,
            ["checkpoint_y"] = CheckpointPosition.Y,
        };

        using FileAccess file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
        if (file == null)
        {
            GD.PushError($"GameState: no se pudo escribir el guardado ({FileAccess.GetOpenError()}).");
            return;
        }
        file.StoreString(Json.Stringify(data));
    }

    public void LoadGame()
    {
        if (!FileAccess.FileExists(SavePath))
        {
            return;
        }

        using FileAccess file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PushError($"GameState: no se pudo leer el guardado ({FileAccess.GetOpenError()}).");
            return;
        }

        Variant parsed = Json.ParseString(file.GetAsText());
        if (parsed.VariantType != Variant.Type.Dictionary)
        {
            GD.PushError("GameState: el archivo de guardado está corrupto; se ignora.");
            return;
        }

        Dictionary data = parsed.AsGodotDictionary();
        Coins = data.ContainsKey("coins") ? (int)data["coins"] : 0;
        HasKey = data.ContainsKey("has_key") && (bool)data["has_key"];

        _abilities.Clear();
        if (data.ContainsKey("abilities"))
        {
            foreach (Variant ability in data["abilities"].AsGodotArray())
            {
                _abilities.Add(ability.AsString());
            }
        }

        _collectedItems.Clear();
        if (data.ContainsKey("collected_items"))
        {
            foreach (Variant itemId in data["collected_items"].AsGodotArray())
            {
                _collectedItems.Add(itemId.AsString());
            }
        }

        CheckpointScene = data.ContainsKey("checkpoint_scene") ? data["checkpoint_scene"].AsString() : "";
        float x = data.ContainsKey("checkpoint_x") ? (float)data["checkpoint_x"] : 0f;
        float y = data.ContainsKey("checkpoint_y") ? (float)data["checkpoint_y"] : 0f;
        CheckpointPosition = new Vector2(x, y);
    }

    /// Borra la partida guardada y resetea el estado (p. ej. para "Nueva partida").
    public void ResetProgress()
    {
        Coins = 0;
        HasKey = false;
        _abilities.Clear();
        _collectedItems.Clear();
        CheckpointScene = "";
        CheckpointPosition = Vector2.Zero;
        if (FileAccess.FileExists(SavePath))
        {
            DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(SavePath));
        }
        EmitSignal(SignalName.CoinsChanged, Coins);
        EmitSignal(SignalName.KeyChanged, HasKey);
    }
}
