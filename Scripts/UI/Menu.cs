using Godot;
using System;

public partial class Menu : Control
{
    PackedScene level1;
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
        GetTree().Root.GetNode<MouseWithJoystick>("MouseWithJoystick").inmenu = true;
        level1 = GD.Load<PackedScene>("res://Scenes/Levels/Level1.tscn");
    }

    public void OnStartButtonPressed()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        GetTree().ChangeSceneToPacked(level1);
    }
    public void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}
