using Godot;
using System;
[Tool]
public partial class Spikes : Sprite2D
{
    [Export]
    public CollisionShape2D CollisionShape;

    public override void _Ready()
    {
        // Set the collision shape to be the same size as the sprite
        RectangleShape2D rectangleShape = new RectangleShape2D();
        rectangleShape.Size = new Vector2( this.RegionRect.Size.X, this.RegionRect.Size.Y/3);
        // Transform2D transformNode = CollisionShape.Transform;
        // transformNode.Y = new Vector2(0, this.RegionRect.Size.Y / 12);
        // CollisionShape.Transform = transformNode;
        CollisionShape.Shape = rectangleShape;
    }
}
