using Godot;
using System;

public partial class ReturnToFortress : Control
{
	public static PackedScene PlayerScene { get; private set; }
	public static PackedScene ProjectileScene { get; private set; }

	public override void _Ready()
	{
		PlayerScene = GD.Load<PackedScene>("res://node/gordon.tscn");
		ProjectileScene = GD.Load<PackedScene>("res://node/projectile.tscn");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
