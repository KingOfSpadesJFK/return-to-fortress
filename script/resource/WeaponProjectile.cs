using Godot;
using System;

[GlobalClass]
public partial class WeaponProjectile : Weapon
{
	[Export] public ProjectileInfo ProjectileToFire { get => _projectile; set => _projectile = value; }
	[Export] public float Speed = 100.0f;
    [Export] public float Spread = 0.0f;
    [Export] public int Count = 1;

	private ProjectileInfo _projectile;

	public override void Fire(Node3D source, Basis initBasis, Vector3 initPos) {
		Projectile instance = ReturnToFortress.Instance.ProjectileScene.Instantiate<Projectile>();
		instance.GlobalTransform = new Transform3D(initBasis, initPos);
		instance.Info = _projectile;
		instance.Speed = Speed;
		if (source is Player player) instance.PlayerInfo = player.Info;
		source.GetParent().AddChild(instance);
		ReturnToFortress.LogInfo("Fired at ", initPos);
	}
}
