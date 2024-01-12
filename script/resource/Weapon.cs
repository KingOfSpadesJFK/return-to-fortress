using Godot;
using System;

[GlobalClass]
public partial class Weapon : Resource
{
    [Export] FireType WeaponFire;
    [Export] public int MaxAmmo = 100;
    [Export] public int MaxClip = 10;
    [Export] public float FireRate = 0.1f;
    public int Ammo { get => _ammo; }

    private int _ammo = 10;
}
