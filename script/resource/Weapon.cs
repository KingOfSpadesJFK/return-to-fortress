using Godot;
using System;

public partial class Weapon : Resource
{
    [Export] public int MaxAmmo = 100;
    [Export] public int MaxClip = 10;
    [Export] public float FireRate = 0.1f;
    public int Ammo { get => _ammo; }

    private int _ammo = 10;

    public virtual void Fire() {}
    public virtual void Fire(Node node, Basis initBasis, Vector3 initPos) {}
}
