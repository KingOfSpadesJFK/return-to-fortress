using Godot;
using System;

public partial class SpawnBox : Area3D
{
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	public void SpawnPlayer(Player player)
	{
		player.Rotation = GlobalRotation;
		if (GetChild<CollisionShape3D>(0).Shape is BoxShape3D box) {
			var random = ReturnToFortress.Instance.Random.NextSingle();
			Vector3 offset = new Vector3(random, random, random) * box.Size;
			player.GlobalPosition = GlobalPosition + offset;
			player.GlobalRotation = GlobalRotation;
		}

		GetNode("/root/Control/SubViewportContainer/MapRoot").AddChild(player);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
