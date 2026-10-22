# Diseño del Mapa para Tidebound

## 1. Estructura del Mundo
El mapa sigue una estructura interconectada con progresión basada en mejoras y backtracking. Las zonas principales son:

### **Zona Inicial - Isla Tutorial**
- Introduce las mecánicas básicas: movimiento, salto, ataque.
- El jugador encuentra su primer objetivo y aprende a usar el gancho.
- Aquí se obtiene el **Gancho Pirata** (el jugador ya empieza con él funcional en el build actual).
- Salida bloqueada hasta conseguir el **Gancho Pirata**, que actúa como "examen" final del tutorial.

### **Cueva de los Ecos**
- Oscura, con caminos alternativos y trampas.
- Enemigos: Murciélagos y cangrejos gigantes.
- El **Gancho Pirata** es la mecánica central de la zona: secciones que exigen dominar el balanceo para alcanzar nuevas áreas.

### **Fortaleza Pirata**
- Vertical, llena de enemigos y patrullas.
- Secciones que requieren el gancho para progresar.
- Contiene la **Pistola Antigua**, que permite activar interruptores a distancia.

### **Galeón Fantasma**
- Zona con plataformas móviles y enemigos espectrales.
- El jugador debe resolver un puzzle para encontrar la **Llave Maldita**.
- La llave desbloquea una entrada a las **Ruinas Hundidas**.

### **Ruinas Hundidas**
- Área submarina con desplazamiento lento.
- Trampas y enemigos que se mueven más rápido que el jugador.
- Aquí se obtiene el **Doble Salto**, necesario para acceder a la siguiente área.

### **Isla de la Tormenta (Zona Final)**
- Entorno peligroso con rayos y fuertes vientos.
- Se requiere usar todas las habilidades aprendidas.
- Enfrentamiento con el **Jefe Final** en el volcán.

---

## 2. Diseño del Flujo del Mapa
El juego permite la exploración libre, pero algunas áreas requieren mejoras para ser accesibles:
```
[Isla Tutorial] → (Gancho) → [Cueva de los Ecos] → [Fortaleza Pirata] → (Pistola) → [Galeón Fantasma] → (Llave) → [Ruinas Hundidas] → (Doble Salto) → [Isla de la Tormenta]
```
- **Atajos y conexiones**: Se desbloquean caminos alternativos para reducir el backtracking.
- **Puntos de guardado**: Colocados estratégicamente en zonas seguras.

---

## 3. Herramientas para Diseñar el Mapa
Para construir el mapa se pueden usar:
- **Papel y lápiz**: Rápido y flexible para bocetos iniciales.
- **Tiled**: Editor de mapas en 2D.
- **Draw.io o Figma**: Para diagramas estructurales.
- **Aseprite/Krita**: Para pixel art si se desea visualizar mejor.

---

Este diseño garantiza un flujo lógico y progresión clara en el metroidvania, combinando exploración, mejoras y desafíos de plataforma.

