using Godot;
using System;

public partial class MouseWithJoystick : Node
{
    [Export]
    public float JoystickSpeed = 200.0f;

    [Export]
    public float ClampRadius = 100.0f;

    // Factor de velocidad del ratón (GetLastMouseVelocity ya viene en px/s).
    // Antes el export MouseSensitivity se sobrescribía cada frame a 200f/1f;
    // ahora hay una constante por dispositivo y el export engañoso desaparece.
    private const float MouseSpeedFactor = 1f;

    public Texture2D CustomCursorTexture;

    public Vector2 virtualCursorPosition;
    private Sprite2D cursorSprite;
    private float inactivityTimer = 0f;
    private float InactivityTime = 2f; // Tiempo antes de ocultar por inactividad

    public Node2D playerNode;
    private Vector2 previousPlayerPosition;

    public Vector2 VirtualCursorPosition => virtualCursorPosition;

    public override void _Ready()
    {
        // Si la escena de arranque no tiene jugador (p. ej. el menú), no inicializamos:
        // los niveles llaman a Initialize() al entrar y el _Process se auto-inicializa
        // en cuanto aparece un jugador. Evita el falso error al arrancar en el menú.
        if (GetTree().GetNodesInGroup("Player").Count > 0)
        {
            Initialize();
        }
    }

    /// <summary>
    /// Inicializa el cursor virtual. Es IDEMPOTENTE: puede llamarse al entrar en
    /// cada escena (Level0/Level1) sin crear sprites de crosshair duplicados.
    /// - Parte "por escena": re-resuelve el jugador y resetea la posición del cursor.
    /// - Parte "una sola vez": crea el sprite solo si no existe ya uno válido.
    /// </summary>
    public void Initialize()
    {
        CustomCursorTexture ??= GD.Load<Texture2D>("res://Assets/Crosshair/crosshair.png");

        var players = GetTree().GetNodesInGroup("Player");
        if (players.Count > 0 && players[0] is Node2D pNode)
        {
            playerNode = pNode;
            virtualCursorPosition = playerNode.GlobalPosition;
            previousPlayerPosition = playerNode.GlobalPosition;
        }
        else
        {
            GD.PrintErr("Error: No se encontró un nodo en el grupo 'Player' o no es un Node2D.");
            virtualCursorPosition = GetViewport().GetVisibleRect().Size / 2.0f;
            previousPlayerPosition = virtualCursorPosition;
        }

        Input.MouseMode = Input.MouseModeEnum.Captured;

        if (CustomCursorTexture != null && (cursorSprite == null || !GodotObject.IsInstanceValid(cursorSprite)))
        {
            cursorSprite = new Sprite2D
            {
                Texture = CustomCursorTexture,
                Centered = true,
            };
            cursorSprite.ZIndex = 1000;
            cursorSprite.Scale = new Vector2(0.75f, 0.75f);

            AddChild(cursorSprite);
        }

        if (cursorSprite != null)
        {
            cursorSprite.GlobalPosition = virtualCursorPosition;
            cursorSprite.Visible = true;
        }
        inactivityTimer = 0f;
    }


    private bool IsUsingJoystick = false; 
    private bool IsUsingMouse = false;
    public bool inmenu = false;


    public override void _Process(double delta)
    {
        if (inmenu)
        {
            return;
        }

        // El jugador puede haberse liberado al cambiar de escena: evitamos referencias colgadas
        if (playerNode != null && !GodotObject.IsInstanceValid(playerNode))
        {
            playerNode = null;
        }

        // Auto-inicialización perezosa: al entrar en una escena con jugador (aunque
        // esa escena no llame a Initialize(), p. ej. Level0), re-resuelve todo.
        if (playerNode == null && GetTree().GetFirstNodeInGroup("Player") is Node2D)
        {
            Initialize();
        }
        Vector2 joystickInput = GetJoystickMovement();
        Vector2 mouseDelta = Input.GetLastMouseVelocity();

        bool joystickMoved = joystickInput.LengthSquared() > 0.5f;
        bool mouseMoved = mouseDelta.LengthSquared() > 0.5f;

        Vector2 playerPosition = Vector2.Zero;
        Vector2 playerMovementDelta = Vector2.Zero;

        if (playerNode != null)
        {
            playerPosition = playerNode.GlobalPosition;
            playerMovementDelta = playerPosition - previousPlayerPosition;
        }

        // *** Lógica para que el cursor siga al jugador cuando está inactivo de INPUT ***
      
        if (!joystickMoved && !mouseMoved)
        {
            virtualCursorPosition += playerMovementDelta;
           
        }

        // *** Lógica de movimiento del cursor por INPUT ***
        if (joystickMoved)
        {
            // La velocidad del joystick la marca JoystickSpeed (export)
            Vector2 movement = joystickInput.Normalized() * JoystickSpeed * (float)delta;
            virtualCursorPosition += movement;
            IsUsingJoystick = true;
            IsUsingMouse = false;
            inactivityTimer = 0f; // *** Reinicia el timer solo con input de joystick ***
        }
        else if (mouseMoved)
        {
            // GetLastMouseVelocity ya devuelve px/s: el factor del ratón es 1
            virtualCursorPosition += mouseDelta * MouseSpeedFactor * (float)delta;
            IsUsingMouse = true;
            IsUsingJoystick = false;
            inactivityTimer = 0f; // *** Reinicia el timer solo con input de ratón ***
        }


        // *** Lógica de reset de posición al jugador si deja de usar joystick ***

        if (IsUsingJoystick && !joystickMoved && !mouseMoved)
        {
            if (playerNode != null)
            {
                virtualCursorPosition = playerNode.GlobalPosition;
            }
        }


        // *** LÓGICA DE AVANCE DEL TIMER DE INACTIVIDAD ***
        if (!joystickMoved && !mouseMoved)
        {
            inactivityTimer += (float)delta;
        }
       


        // LÓGICA DE CLAMP: círculo de radio ClampRadius alrededor del jugador.
        // (El clamp cuadrado posterior era redundante: el círculo ya está contenido en el cuadrado)
        if (playerNode != null)
        {
            Vector2 vectorToCursor = virtualCursorPosition - playerPosition;
            float currentDistance = vectorToCursor.Length();

            if (currentDistance > ClampRadius)
            {
                vectorToCursor = vectorToCursor.Normalized() * ClampRadius;
                virtualCursorPosition = playerPosition + vectorToCursor;
            }
        }

        // Actualiza la posición anterior del jugador
        if (playerNode != null)
        {
            previousPlayerPosition = playerPosition;
        }

        // *** Lógica de VISIBILIDAD FINAL basada SÓLO en el timer ***
        if (cursorSprite != null)
        {
            cursorSprite.GlobalPosition = virtualCursorPosition; 
            cursorSprite.Visible = inactivityTimer < InactivityTime;
        }
    }

    private Vector2 GetJoystickMovement()
    {
        float moveX = Input.GetAxis("mover_horizontal_minus", "mover_horizontal_plus");
        float moveY = Input.GetAxis("mover_vertical_minus", "mover_vertical_plus");
        return new Vector2(moveX, moveY);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationExitTree)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
            Input.SetCustomMouseCursor(null);
        }
    }
}