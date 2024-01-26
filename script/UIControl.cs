using Godot;
using System;

public partial class UIControl : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void _on_host_button_pressed()
	{
		ReturnToFortress.LogInfo("Hosting server...");
		ReturnToFortress.Instance.GotoMap("res://map/2fort.tscn");
	}
}
