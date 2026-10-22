# Implementación de Mecánica de Gancho en Godot (C#)

## 1. Usando Raycast + Movimiento (Gancho Rápido y Preciso)

Este método usa un `RayCast2D` para detectar superficies enganchables y mueve al personaje hacia el punto enganchado.

### **Código en C#**

```csharp
using Godot;
public partial class Player : CharacterBody2D
{
    private bool isHooked = false;
    private Vector2 hookTarget;
    private float hookSpeed = 800f;

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("shoot_hook"))
        {
            var space = GetWorld2D().DirectSpaceState;
            var query = PhysicsRayQueryParameters2D.Create(GlobalPosition, GetGlobalMousePosition());
            var result = space.IntersectRay(query);

            if (result.Count > 0) // Si golpea algo
            {
                hookTarget = (Vector2)result["position"];
                isHooked = true;
            }
        }

        if (isHooked)
        {
            GlobalPosition = GlobalPosition.MoveToward(hookTarget, hookSpeed * (float)delta);
            if (GlobalPosition.DistanceTo(hookTarget) < 10) // Cuando llega al punto
            {
                isHooked = false;
            }
        }
    }
}
```

### **Ventajas**

✅ Responde rápido y es fácil de controlar.\
✅ Ideal para juegos de plataformas de acción.\
⚠ No tiene balanceo físico, el movimiento es directo.

---

## 2. Usando Joints (Gancho con Física y Balanceo)

Este método usa `PinJoint2D` para conectar el personaje al punto enganchado, permitiendo un balanceo más realista.

### **Código en C#**

```csharp
using Godot;
public partial class Player : RigidBody2D
{
    private PinJoint2D hookJoint;
    private bool isHooked = false;

    public override void _Ready()
    {
        hookJoint = GetNode<PinJoint2D>("HookJoint");
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("shoot_hook"))
        {
            var space = GetWorld2D().DirectSpaceState;
            var query = PhysicsRayQueryParameters2D.Create(GlobalPosition, GetGlobalMousePosition());
            var result = space.IntersectRay(query);

            if (result.Count > 0) // Si golpea algo
            {
                hookJoint.GlobalPosition = (Vector2)result["position"];
                hookJoint.Enabled = true;
                isHooked = true;
            }
        }

        if (Input.IsActionJustPressed("release_hook") && isHooked)
        {
            hookJoint.Enabled = false;
            isHooked = false;
        }
    }
}
```

### **Ventajas**

✅ Simulación física realista con balanceo.\
✅ Da más dinamismo a la movilidad.\
⚠ Puede ser más difícil de controlar si no se ajusta bien.

---

## 3. Haciendo el Gancho Visible

Para que el gancho no sea invisible, podemos agregar un **sprite** y una **cuerda visual** usando `Line2D`.

### **Dibujar la cuerda con Line2D**
```csharp
using Godot;
public partial class Hook : Line2D
{
    private Vector2 start;
    private Vector2 end;
    private bool isHooked = false;

    public void SetHook(Vector2 startPos, Vector2 endPos)
    {
        start = startPos;
        end = endPos;
        isHooked = true;
        UpdateLine();
    }

    public void ReleaseHook()
    {
        isHooked = false;
        ClearPoints();
    }

    public override void _Process(double delta)
    {
        if (isHooked)
        {
            UpdateLine();
        }
    }

    private void UpdateLine()
    {
        ClearPoints();
        AddPoint(start);
        AddPoint(end);
    }
}
```

### **Añadir un Sprite al Gancho**
```csharp
hookSprite.GlobalPosition = hookTarget;
hookSprite.Visible = true;
```

### **Resultado**
- Se verá una **cuerda** conectando al jugador con el punto enganchado.\
- El **gancho** tendrá un sprite visible al impactar.\
- Si el jugador suelta el gancho, la cuerda desaparecerá.

---

## ¿Cuál usar?

✅ **Raycast + Movimiento** → Para un gancho rápido y preciso, estilo Bionic Commando.\
✅ **PinJoint2D + Física** → Para un gancho más realista, con balanceo dinámico.

📌 **Consejo:** Prueba primero la versión con Raycast para asegurar que la mecánica base funciona. Luego, si quieres algo más dinámico, implementa `PinJoint2D`.

Si necesitas ajustes o quieres combinar mecánicas, ¡haz pruebas y personaliza el código a tu juego! 🚀🏴‍☠️

