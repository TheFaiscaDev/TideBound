# Character State Machine

- **Idle**
  - → Run (Si se presiona izquierda/derecha)
  - → Jump (Si el jugador salta)
  - → Attack (Si el jugador ataca)
  - → Hook (Si usa el gancho)
  - → Hurt (Si recibe daño)

- **Run**
  - → Idle (Si deja de moverse)
  - → Jump (Si el jugador salta)
  - → Attack (Si el jugador ataca)
  - → Hook (Si usa el gancho)
  - → Hurt (Si recibe daño)

- **Jump**
  - → Fall (Si la velocidad vertical es negativa)
  - → Attack (Si ataca en el aire)
  - → Hook (Si usa el gancho en el aire)
  - → Hurt (Si recibe daño)

- **Fall**
  - → Idle (Si toca el suelo sin moverse)
  - → Run (Si toca el suelo moviéndose)
  - → Hook (Si usa el gancho en el aire)
  - → Hurt (Si recibe daño)

- **Attack**
  - → Idle / Run (Cuando termina la animación)
  - → Hurt (Si recibe daño)

- **Hook**
  - → Fall (Si suelta el gancho en el aire)
  - → Idle / Run (Si aterriza tras usarlo)
  - → Hurt (Si recibe daño)

- **Hurt**
  - → Idle / Run (Si aún tiene vida y está en el suelo)
  - → Fall (Si está en el aire)
  - → Dead (Si la vida llega a 0)

- **Dead** (Fin del juego)
