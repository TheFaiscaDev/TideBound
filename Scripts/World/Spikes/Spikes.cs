using Godot;
using System;
[Tool]
public partial class Spikes : Sprite2D
{
    [Export]
    public CollisionShape2D CollisionShape;

    public override void _Ready()
    {
        // La hitbox cubre el tercio inferior del sprite (la base de los pinchos).
        // Se crea una shape por instancia porque el tamaño depende del RegionRect.
        CollisionShape.Shape = new RectangleShape2D
        {
            Size = new Vector2(RegionRect.Size.X, RegionRect.Size.Y / 3)
        };
    }
}
