using Godot;
using System;

public partial class MouseWithJoystick : Node
{
    [Export]
    public float JoystickSpeed = 200.0f;

    [Export]
    public float MouseSensitivity = 80.0f;

    [Export]
    public float ClampRadius = 100.0f;

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
        Initialize();
    }

    public void Initialize()
    {

        CustomCursorTexture = GD.Load<Texture2D>("res://Assets/Crosshair/crosshair.png");

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

        if (CustomCursorTexture != null)
        {
            cursorSprite = new Sprite2D
            {
                Texture = CustomCursorTexture,
                Centered = true,
            };
            cursorSprite.ZIndex = 1000;
            cursorSprite.Scale = new Vector2(0.75f, 0.75f);

            AddChild(cursorSprite);
            cursorSprite.GlobalPosition = virtualCursorPosition;
            cursorSprite.Visible = true;
        }
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
            MouseSensitivity=200f; // *** Aumenta la sensibilidad del joystick ***
            Vector2 movement = joystickInput.Normalized() * JoystickSpeed * (float)delta;
            virtualCursorPosition += movement;
            IsUsingJoystick = true;
            IsUsingMouse = false;
            inactivityTimer = 0f; // *** Reinicia el timer solo con input de joystick ***
        }
        else if (mouseMoved)
        {
            MouseSensitivity = 1f; // *** Baja la sensibilidad del ratón ***
            virtualCursorPosition += mouseDelta * MouseSensitivity * (float)delta;
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
                inactivityTimer += (float)delta;
            
            }
        }


        // *** LÓGICA DE AVANCE DEL TIMER DE INACTIVIDAD ***
        if (!joystickMoved && !mouseMoved)
        {
            inactivityTimer += (float)delta;
        }
       


        // LÓGICA DE CLAMP (alrededor del jugador, luego a la pantalla) - Como antes
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

        if (playerNode != null)
        {
            Vector2 halfArea = new Vector2(ClampRadius, ClampRadius); // Define el área cuadrada alrededor del jugador
            Vector2 minBounds = playerNode.GlobalPosition - halfArea;
            Vector2 maxBounds = playerNode.GlobalPosition + halfArea;

            virtualCursorPosition = virtualCursorPosition.Clamp(minBounds, maxBounds);
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