using Godot;
using System;

public partial class Chest : RigidBody2D
{
    private AnimatedSprite2D animatedSprite;

    public bool isOpen = false;
    private PackedScene coinScene;

    public override void _Ready()
    {
        coinScene = GD.Load<PackedScene>("res://Scenes/Coin/coin.tscn");
        animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }
    /// Abre el cofre. Devuelve true solo la primera vez que se abre.
    public bool Open()
    {
        if (isOpen)
        {
            return false;
        }
        animatedSprite.Play("open");
        SpawnCoins();
        isOpen = true;
        SetCollisionMaskValue(1, false);
        GravityScale = 0;
        return true;
    }

    private void SpawnCoins()
    {
        for (int i = 0; i < 5; i++)
        {
            RigidBody2D coin = (RigidBody2D)coinScene.Instantiate();
            GetParent().AddChild(coin);

            // Posición inicial cercana al cofre
            Vector2 offset = new Vector2(GD.Randf() * 30 - 15, GD.Randf() * -10);
            coin.GlobalPosition = GlobalPosition + offset;
            float xImpulse = GD.RandRange(-300, 300);
            float yImpulse = GD.RandRange(-600, -200);
            Vector2 impulse = new Vector2(xImpulse, yImpulse);

            coin.ApplyImpulse(Vector2.Zero, impulse);
        }
    }
}

