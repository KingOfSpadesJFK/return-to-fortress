using Godot;
using System;

public partial class DebugHUD : RichTextLabel
{
	public Player Player { get => _player; set => _player = value; }
	private Player _player;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		this.Clear();
		this.AddText("UniquePeerID: " + ReturnToFortress.Instance.UniquePeerID + "\n");
		if (_player is not null) {
			this.AddText("Health: " + _player.Info.Health + "\n");
			this.AddText("Armor: " + _player.Info.Armor + "\n");
			this.AddText("Ammo: " + _player.Info.Weapon.Ammo + "/" + _player.Info.Weapon.MaxAmmo + "\n");
			this.AddText("Velocity: (" + string.Format("{0:0.00}", _player.Velocity.X) + ", " +
					string.Format("{0:0.00}", _player.Velocity.Y) + ", " +
					string.Format("{0:0.00}", _player.Velocity.Z) + ")\n");
			this.AddText("Speed: " + string.Format("{0:0.00}", new Vector2(_player.Velocity.X, _player.Velocity.Z).Length()) + "\n");

			this.AddText("Loadout: " + _player.Info.Loadout + "\n");
			this.AddText("Weapon: " + _player.Info.Weapon + "\n");
		}
	}
}
