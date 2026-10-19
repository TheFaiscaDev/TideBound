using Godot;
using System;

public partial class Rope : Line2D
{
	[Export] public Vector2 start;
	[Export] public Vector2 end;

	public override void _Process(double delta)
	{
		base._Process(delta);
		if (this.Visible)
		{
			// Actualizamos los puntos en vez de crearlos y borrarlos cada frame
			if (GetPointCount() != 2)
			{
				ClearPoints();
				AddPoint(start);
				AddPoint(end);
			}
			else
			{
				SetPointPosition(0, start);
				SetPointPosition(1, end);
			}
		}
		else if (GetPointCount() > 0)
		{
			ClearPoints();
		}
	}

}
