using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class FadeArea : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnBodyEntered(Node2D body)
	{
		if(body is Player)
		{
			TileMapLayer parent = (TileMapLayer)this.GetParent();
			Color parentModulate = Color.Color8(255, 255, 255, 255); ;

			if(parent != null)
			{
				parentModulate.A = 0.1f;
				parent.Enabled = false;
				parent.Modulate = parentModulate;
			}

		}
	}
    public void OnBodyExited(Node2D body)
    {
        if (body is Player)
        {
            TileMapLayer parent = (TileMapLayer)this.GetParent();
            Color parentModulate = Color.Color8(255,255, 255, 255);

            if (parent != null)
            {
                parent.Enabled = true;
                parentModulate.A = 1;
                parent.Modulate = parentModulate;
            }

        }
    }
}
