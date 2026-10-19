using Godot;
using Godot.NativeInterop;
using System;

public partial class Camera : Camera2D
{
	[Export] // El zoom por defecto cuando no estás en ningún trigger
	public Vector2 DefaultZoom = Vector2.One; // Vector2(1, 1) es zoom normal

	[Export] // La duración del zoom suave (en segundos)
	public float ZoomDuration = 1.0f;

	private Tween _zoomTween; // Para gestionar la animación suave del zoom
	private CharacterBody2D _player; // Referencia cacheada al jugador

	public override void _Ready()
	{
		// Aseguramos que el zoom inicial es el por defecto
		Zoom = DefaultZoom;
		_player = GetTree().GetFirstNodeInGroup("Player") as CharacterBody2D;
	}

	// Este método será llamado por los triggers para solicitar un cambio de zoom
	public void RequestZoom(Vector2 targetZoom)
	{
		// Si ya hay una animación de zoom en curso, la detenemos bruscamente
		if (_zoomTween != null && _zoomTween.IsValid())
		{
			_zoomTween.Kill();
		}

		// Creamos un nuevo Tween
		_zoomTween = CreateTween();

		// Animamos la propiedad 'zoom' desde el valor actual de la cámara
		// hasta el targetZoom, durante la ZoomDuration.
		// set_trans y set_ease definen el tipo de suavizado (curva de animación)
		_zoomTween.TweenProperty(this, "zoom", targetZoom, ZoomDuration)
			.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);

		// Opcional: Si necesitas saber cuándo termina el zoom, puedes conectar una señal
		// _zoomTween.Finished += OnZoomFinished;
	}

	// Método opcional si conectas la señal Finished
	// private void OnZoomFinished()
	// {
	//     GD.Print("Zoom animation finished!");
	// }
	

	public override void _Process(double delta)
	{
		if (_player == null || !GodotObject.IsInstanceValid(_player))
		{
			return;
		}
		Vector2 position = _player.GlobalPosition;
		position.Y -= 20;
		GlobalPosition = position;
	}
}
