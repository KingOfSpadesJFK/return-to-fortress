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

	private void HideButtons()
	{
		GetNode<Button>("Menu/HostButton")?.Hide();
		GetNode<Button>("Menu/JoinButton")?.Hide();
	}

	private void _on_host_button_pressed()
	{
		ReturnToFortress.LogInfo("Hosting server...");
		if (ReturnToFortress.Instance.HostServer() == Error.Ok) HideButtons();
	}
	private void _on_join_button_pressed()
	{	
		ReturnToFortress.LogInfo("Joining server...");
		if (ReturnToFortress.Instance.JoinServer() == Error.Ok) HideButtons();
	}
}
