using Godot;
using System;

[GlobalClass]
public partial class Weapon : Resource
{
    [Export] public string Name = "Weapon";
    [Export] public int MaxAmmo = 100;
    [Export] public int MaxClip = 10;
    [Export] public float FireRate = 0.1f;
    [Export] public Vector3 FirePointOffset = new Vector3(0.0f, 0.0f, 0.0f);
    [Export] public Mesh WeaponMesh;
    [Export] public Skin WeaponSkin;
    [Export] public Animation IdleAnimation;
    [Export] public Animation FireAnimation;
    [Export] public Animation ReloadAnimation;
    [Export] public bool LoopReloadAnimation = false;
    [Export] public int ReloadAmmoPerLoop = 1;
    [Export] public Animation EquipAnimation;
    [Export] public Animation UnequipAnimation;
    [Export] public Mesh[] MuzzleflashMeshes;
    [Export] public Skin MuzzleflashSkin;
    [Export] public ParticleProcessMaterial MuzzleflashParticleMaterial;
    
    public int Ammo { get => _ammo; }

    private int _ammo = 10;

    public virtual void Fire() {}
    public virtual void Fire(Node3D source, Basis initBasis, Vector3 initPos) {}
    public override string ToString() { return Name; }
}
