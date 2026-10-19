using Godot;
using System;

public partial class ZoomTrigger : Area2D
{
    [Export] // El nivel de zoom al que la cámara debe cambiar al entrar en esta área
    public Vector2 TargetZoom = new Vector2(0.5f, 0.5f); // Ejemplo: zoom In a 50%

    [Export] // El NodePath a tu nodo Camera2D en el árbol de escenas
    // Arrastra tu nodo Camera2D desde el árbol de escenas a esta propiedad en el Inspector
    public NodePath CameraPath;

    private Camera _cameraZoomer; // Referencia al script de la cámara

    public override void _Ready()
    {
        // Obtener la referencia al script de la cámara usando el NodePath
        if (CameraPath != null)
        {
            _cameraZoomer = GetNode<Camera>(CameraPath);
            if (_cameraZoomer == null)
            {
                GD.PrintErr($"ZoomTrigger: No se encontró un nodo CameraZoomer en la ruta: {CameraPath}");
            }
        }
        else
        {
            GD.PrintErr("ZoomTrigger: La propiedad CameraPath no está asignada.");
        }


        // Conectar las señales de entrada y salida del cuerpo (Body)
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        // IMPORTANTE: Configura las capas de colisión y máscara de este Area2D
        // para que interactúe con el tipo de cuerpo que representa a tu jugador.
        // Asegúrate de que tu nodo jugador tenga también la capa de colisión adecuada.
    }

    // Se llama cuando un cuerpo entra en el Area2D
    private void OnBodyEntered(Node2D body)
    {
        // Asegúrate de que el cuerpo que entró es el jugador (por ejemplo, usando un grupo)
        // Necesitas añadir tu nodo jugador al grupo "player" en el Inspector
        if (body.IsInGroup("Player"))
        {
            if (_cameraZoomer != null)
            {
                // Solicita a la cámara que haga zoom al TargetZoom de este trigger
                _cameraZoomer.RequestZoom(TargetZoom);
            }
        }
    }

    // Se llama cuando un cuerpo sale del Area2D
    private void OnBodyExited(Node2D body)
    {
        // Asegúrate de que el cuerpo que salió es el jugador
        if (body.IsInGroup("Player"))
        {
            // Aquí, si el jugador sale, le pedimos a la cámara que vuelva a su zoom por defecto.
            // Nota: Esto es una implementación simple. Si tienes triggers anidados,
            // necesitarías una lógica más compleja en la cámara para saber a qué zoom
            // volver (por ejemplo, el del trigger padre).
            if (_cameraZoomer != null)
            {
                _cameraZoomer.RequestZoom(_cameraZoomer.DefaultZoom);
            }
        }
    }
}