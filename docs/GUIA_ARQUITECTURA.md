# 🏗️ Guía de Arquitectura — TideBound

Esta guía explica **paso a paso** cómo crear cualquier elemento del juego (enemigos, objetos, componentes, interfaces, escenas) manteniendo la arquitectura del proyecto. Síguela cada vez que añadas algo nuevo.

---

## 1. Estructura de carpetas

```
Scripts/
├── Actors/            # Entidades con comportamiento propio (se mueven, actúan)
│   ├── Player/        # Player.cs, CompMov_Player.cs, HookComponent.cs, LineRope/
│   └── Enemies/       # Enemy.cs (base) + States/ (estados genéricos) + una carpeta por enemigo
│       ├── States/    # Estados genéricos: PatrolPathState, TimedIdleState, HitStunState, MeleeAttackState
│       ├── PinkyStar/ # PinkyStar.cs + States/ (subclases finas de los genéricos)
│       └── BigRed/
├── Components/        # Piezas reutilizables que se enchufan a cualquier nodo
│   ├── HealthComponent.cs
│   ├── DamageComponent.cs
│   ├── KnockbackComponent.cs
│   └── ItemCollector.cs
├── Interfaces/        # Contratos C# (IDamageable, IAttackable, IHook)
├── StateMachine/      # StateMachine.cs y State.cs (genéricos, no se tocan)
├── Autoload/          # Singletons: RespawnManager, UIManager, MouseWithJoystick, GameState
├── UI/                # UserInterface.cs, Menu.cs
├── Items/             # Objetos interactivos: Coin, Key, Chest, Box
├── World/             # Elementos del escenario: Camera, Spikes, FadeArea, ZoomTrigger...
└── Levels/            # Scripts de cada nivel (Level1.cs...)
```

**Regla de oro:** un script va en la carpeta de *lo que es*, no de *donde se usa*. Los scripts NUNCA viven dentro de `Scenes/`.

---

## 2. Las 4 piezas de la arquitectura

### 2.1. ¿Cuándo usar HERENCIA? (clase base)

Cuando compartes **el "ser"** de una entidad: todo enemigo *es* un `Enemy`, que *es* un `CharacterBody2D`.

```csharp
// La base define el QUÉ y el comportamiento común
public partial class Enemy : CharacterBody2D, IDamageable { ... }

// La subclase solo define lo que le hace DIFERENTE
public partial class PinkyStar : Enemy { ... }
```

✅ Úsala para: tipos de enemigos, tipos de proyectiles, variantes de una misma entidad.
❌ No la uses para compartir funcionalidad suelta entre cosas no relacionadas (para eso están los componentes).

### 2.2. ¿Cuándo usar COMPOSICIÓN? (componentes)

Cuando compartes **una capacidad** que pueden tener entidades muy distintas: tener vida, recibir retroceso, ser recolectado...

Un componente es un **nodo hijo** con una sola responsabilidad:

```csharp
[GlobalClass] // Para que aparezca en el diálogo "Crear nodo" del editor
public partial class HealthComponent : Node
{
    [Export] public int Health { get; set; } = 100;
    [Signal] public delegate void HealthChangedEventHandler(int newHealth);
    [Signal] public delegate void ObjectDestroyedEventHandler();
    public void TakeDamage(int amount) { ... }
}
```

**Cómo se crea uno nuevo (ejemplo: un `StaminaComponent`):**
1. Crea `Scripts/Components/StaminaComponent.cs` heredando de `Node` (o del tipo que necesite: `Area2D`, `Node2D`...).
2. Añade `[GlobalClass]` si quieres crearlo desde el editor.
3. Expón configuración con `[Export]` y comunícate **hacia afuera con `[Signal]`** (nunca llames directamente a nodos de fuera del componente).
4. En la escena del actor, añade el nodo hijo y configura sus exports en el inspector.
5. El padre lo obtiene en `_Ready()`: `_health = GetNodeOrNull<HealthComponent>("HealthComponent");`

**Regla de dirección de dependencias:** el padre conoce al componente; el componente NO conoce al padre (como mucho `GetParent()` en su `_Ready` si es imprescindible). La comunicación componente → mundo se hace con **señales**.


### 2.3. ¿Cuándo usar INTERFACES?

Cuando quieres **actuar sobre algo sin saber qué es**: la espada daña enemigos, cajas y lo que venga, sin conocerlos.

```csharp
// Scripts/Interfaces/IDamageable.cs
public interface IDamageable
{
    void TakeDamage(int amount);
    void ApplyKnockback(Vector2 sourcePosition, float force, float duration);
}
```

**Cómo se usa:**
1. La entidad la implementa: `public partial class Enemy : CharacterBody2D, IDamageable`
2. Quien interactúa comprueba el tipo, nunca strings:

```csharp
// ✅ BIEN: type-safe, el compilador te protege
if (body is IDamageable target)
{
    target.TakeDamage(1);
    target.ApplyKnockback(GlobalPosition, 100f, 0.2f);
}

// ❌ MAL: frágil, falla en runtime si renombras algo
if (body.HasMethod("TakeDamage")) body.CallDeferred("TakeDamage", 1);
```

**Convenciones:** nombre con prefijo `I` + adjetivo/verbo (`IDamageable`, `IAttackable`, `IHook`). Nunca `AttackInterface` ni `DamageableObject`.

### 2.4. ¿Cuándo usar AUTOLOADS (singletons)?

Solo para **servicios globales** que deben sobrevivir a los cambios de escena:

| Autoload | Responsabilidad |
|---|---|
| `UIManager` | Acceso centralizado a UI, barra de vida y fade de pantalla |
| `RespawnManager` | Spawn/despawn/respawn de enemigos por distancia |
| `MouseWithJoystick` | Cursor virtual con mando |

**Patrón de registro (importante):** los autoloads NO buscan nodos por rutas absolutas. Los nodos **se registran** en su `_Ready()`:

```csharp
// En UserInterface._Ready()
if (GetNodeOrNull<UIManager>("/root/UIManager") is UIManager uiManager)
{
    uiManager.RegisterUI(this);
}

// Quien lo necesite lo consulta:
UserInterface ui = GetNodeOrNull<UIManager>("/root/UIManager")?.UI;
```

❌ NUNCA hagas esto: `GetNode("/root/Level1/Menu/UI/TextureProgressBar")` — se rompe al cambiar de nivel.

**Para registrar un autoload nuevo:** crea el script en `Scripts/Autoload/` y añádelo en `project.godot`:

```ini
[autoload]
MiManager="*res://Scripts/Autoload/MiManager.cs"
```

---

## 3. Receta completa: crear un ENEMIGO nuevo

Vamos a crear un enemigo `Crab` desde cero, siguiendo el patrón de PinkyStar/BigRed.

### Paso 1 — El script de la entidad

`Scripts/Actors/Enemies/Crab/Crab.cs`:

```csharp
using Godot;

public partial class Crab : Enemy
{
    public override void _Ready()
    {
        base._Ready(); // OBLIGATORIO: la base inicializa knockback, vida, spawn y corpse timer
        AddToGroup("Enemy");
    }

    // Reacción al retroceso: pasa al estado Hit
    protected override void OnKnockback()
    {
        GetNode<StateMachine>("FSM").TransitionTo("Hit");
    }

    // Su animación de muerte (una sola vez, la gestiona la base)
    protected override void OnDeathAnimation()
    {
        PlayAnimation("dead");
    }
}
```

**Qué hereda gratis de `Enemy`:** gravedad, `MoveAndSlide` único, `TakeDamage`/`ApplyKnockback` (IDamageable), muerte/despawn, respawn, referencia `player` por grupo, y `PlayAnimation(string)` (sobrescríbelo si usas AnimationTree en vez de AnimatedSprite2D).

### Paso 2 — Los estados (FSM)

Un estado por comportamiento. `Scripts/Actors/Enemies/Crab/States/CrabIdle.cs`:

```csharp
using Godot;

public partial class CrabIdle : State
{
    // El enemigo llega SOLO desde la FSM, nada de asignarlo en el inspector
    private Enemy enemy => GetControlledNode<Enemy>();

    public override void Enter()
    {
        enemy.PlayAnimation("idle");           // animación agnóstica del sistema
        GetNodeOrNull<Timer>("Timer")?.Start();
    }

    public override void Exit()
    {
        GetNodeOrNull<Timer>("Timer")?.Stop(); // SIEMPRE parar timers al salir
    }

    public override void PhysicsUpdate(float delta)
    {
        // lógica por frame de física: solo asigna enemy.Velocity,
        // NUNCA llames a MoveAndSlide (eso es trabajo de la clase base)
    }

    public void OnTimerTimeout()               // handlers en PascalCase
    {
        if (fsm.currentState == this)          // SIEMPRE guard contra transiciones fantasma
        {
            fsm.TransitionTo("Run");
        }
    }
}
```

**Convenciones de estados:**
- Nombre: `<Enemigo><Estado>` en PascalCase (`CrabIdle`, `CrabRun`, `CrabAttack`, `CrabHit`).
- Obtienen al actor con `GetControlledNode<Enemy>()` (la FSM ya tiene asignado `ControlledNode`).
- Los estados **deciden**; la clase base **ejecuta** (física, gravedad, `MoveAndSlide`).


### Paso 3 — La escena del enemigo (`Crab.tscn`)

Estructura de nodos:

```
Crab (CharacterBody2D, script Crab.cs)          ← grupos: ["Enemy"]
├── CollisionShape2D                            ← asignada al export collisionShape
├── AnimatedSprite2D                            ← asignada al export animatedSprite
├── FSM (Node, script StateMachine.cs)
│   ├── InitialState = Idle, ControlledNode = ..
│   ├── Idle (Node, script CrabIdle.cs)
│   │   └── Timer (wait_time = 2.0)
│   ├── Run (Node, script CrabRun.cs)
│   │   └── Timer
│   └── Hit (Node, script CrabHit.cs)
│       └── Timer
├── HealthComponent (Node, script HealthComponent.cs)   ← Health = 5 en el inspector
├── KnockbackComponent (Node, script KnockbackComponent.cs)
└── hurtBox (instancia de Scenes/DamageComponent/damage_component.tscn)
    └── CollisionShape2D
```

**Conexiones de señales (editor, pestaña Nodo → Señales):**
- `HealthComponent.ObjectDestroyed` → `Crab.OnHealthComponentObjectDestroyed`
- `Timer.timeout` de cada estado → `OnTimerTimeout` de ese estado
- `hurtBox.body_entered` → `hurtBox.OnBodyEntered`

**Si el enemigo patrulla:** colócalo bajo un `Path2D → PathFollow2D` en el nivel. El respawn restaura automáticamente el `ProgressRatio` inicial.

### Paso 4 — Colocarlo en el nivel

Instancia `Crab.tscn` en el nivel. **No asignes nada a mano**: el jugador se resuelve por grupo, la vida vive en su `HealthComponent`, el `RespawnManager` lo activa/desactiva y lo respawnea solo.

---

## 4. Receta: crear un OBJETO DAÑABLE (caja, barril, puerta...)

```csharp
public partial class Barrel : RigidBody2D, IDamageable
{
    [Export] public int Health { get; set; } = 1;

    public void TakeDamage(int amount)
    {
        Health -= amount;
        if (Health <= 0) { /* animación de destrucción */ }
    }

    public void ApplyKnockback(Vector2 sourcePosition, float force, float duration)
    {
        // RigidBody2D → impulso; CharacterBody2D → KnockbackComponent
        ApplyImpulse((GlobalPosition - sourcePosition).Normalized() * force);
    }
}
```

Le añades una instancia de `damage_component.tscn` como hurtbox y la espada ya funciona con él **sin tocar la espada**.

---

## 5. Receta: crear un ITEM recolectable

1. Script en `Scripts/Items/MiItem/MiItem.cs` con un método `Destroy()` (o el verbo que aplique: `Open()`, `Consume()`).
2. Añade el comportamiento en `ItemCollector.OnBodyEntered` con pattern matching:

```csharp
switch (body)
{
    case Gem gem:
        gem.Destroy();
        ui.AddCoins(5);
        break;
}
```

Nada de `HasMethod`/`CallDeferred` con strings: si el objeto debe reaccionar, que su tipo lo diga.

---

## 6. Checklist de convenciones (revísalo antes de cada commit)

- [ ] **Clases y métodos públicos en PascalCase**: `UserInterface`, `TakeDamage`, `OnBodyEntered`. Nunca `userInterface`, `open()`, `_on_snake_case`.
- [ ] **Interfaces con prefijo `I`**: `IDamageable`, no `DamageableObject`.
- [ ] **Handlers de señales**: `On<Señal>` (`OnTimerTimeout`, `OnBodyEntered`), conectados desde el editor.
- [ ] **Privados con `_camelCase`**, exports en PascalCase.
- [ ] **Type-safe siempre**: `is IDamageable`, `GetNodeOrNull<T>()` — prohibido `HasMethod`/`CallDeferred` con strings.
- [ ] **Cero rutas absolutas** (`/root/Level1/...`): usa grupos, `UIManager` o referencias exportadas.
- [ ] **Cero logs de debug** (`GD.Print`, `Console.WriteLine`) en código que va a commit.
- [ ] **Un solo `MoveAndSlide` por entidad**, en la clase base.
- [ ] **Física siempre escalada por `delta`** (`velocity += gravity * delta`).
- [ ] Los estados **paran sus timers en `Exit()`** y guardan `fsm.currentState == this` en los timeouts.
- [ ] El componente comunica con **señales**; no conoce a su padre más de lo imprescindible.

---

## 7. Reglas para escenas (.tscn)

1. **Una escena por entidad**, en `Scenes/<Entidad>/`.
2. Los exports de referencias a nodos (`collisionShape`, `animatedSprite`...) se asignan **en la escena de la entidad**, nunca en las instancias del nivel.
3. Las instancias en el nivel **no sobreescriben referencias** (`player = NodePath(...)` está prohibido; si lo necesitas, algo está mal diseñado).
4. Los `.uid` de los scripts se commitean siempre (van con su `.cs`).
5. Mover un script = mover su `.cs` **y** su `.cs.uid` juntos (con `git mv`), y actualizar las rutas en los `.tscn`.

---

## 8. Flujo de daño (referencia rápida)

```
Espada (Area2D swordHit)
  → OnSwordHitAreaEntered / OnSwordHitBodyEntered
  → body is IDamageable
      ├→ target.ApplyKnockback(...) → KnockbackComponent calcula velocidad
      └→ target.TakeDamage(1) → HealthComponent
                                    ├→ HealthChanged → UI
                                    └→ ObjectDestroyed → Enemy.Die()
                                                           ├→ animación muerte (1 vez)
                                                           ├→ colisiones/hurtbox/FSM off
                                                           └→ corpse timer → despawn
                                                                ↓
                                              RespawnManager (si jugador lejos) → ResetEnemy()
```
