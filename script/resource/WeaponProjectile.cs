using Godot;
using System;

[GlobalClass]
public partial class WeaponProjectile : Weapon
{
    [Export] public ProjectileInfo ProjectileToFire { get => _projectile; set => _projectile = value; }
    [Export] public float Speed = 100.0f;
    private ProjectileInfo _projectile;

    public override void Fire(Node node, Basis initBasis, Vector3 initPos, PlayerInfo playerInfo = null) {
        Projectile instance = ReturnToFortress.ProjectileScene.Instantiate<Projectile>();
        instance.GlobalTransform = new Transform3D(initBasis, initPos);
        instance.Info = _projectile;
        instance.Speed = Speed;
        instance.PlayerInfo = playerInfo;
        node.AddChild(instance);
        ReturnToFortress.LogInfo("Fired at ", initPos);
    }
}