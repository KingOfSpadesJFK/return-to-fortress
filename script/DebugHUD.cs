using Godot;
using System;

public partial class DebugHUD : RichTextLabel
{

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		this.Clear();
		this.AddText("UniquePeerID: " + ReturnToFortress.Instance.UniquePeerID + "\n");
		var player = ReturnToFortress.Instance.LocalPlayer;
		if (player is not null) {
			this.AddText("Health: " + player.Info.Health + "\n");
			this.AddText("Armor: " + player.Info.Armor + "\n");
			this.AddText("Ammo: " + player.Info.Weapon.Ammo + "/" + player.Info.Weapon.MaxAmmo + "\n");
			this.AddText("Velocity: (" + string.Format("{0:0.00}", player.Velocity.X) + ", " +
					string.Format("{0:0.00}", player.Velocity.Y) + ", " +
					string.Format("{0:0.00}", player.Velocity.Z) + ")\n");
			this.AddText("Speed: " + string.Format("{0:0.00}", new Vector2(player.Velocity.X, player.Velocity.Z).Length()) + "\n");

			this.AddText("Loadout: " + player.Info.Loadout + "\n");
			this.AddText("Weapon: " + player.Info.Weapon + "\n");
		}
	}
}
