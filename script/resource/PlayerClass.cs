using Godot;
using System;

[GlobalClass]
public partial class PlayerClass : Resource
{
    [Export] public string Name = "Scout";
    [Export] public int MaxHealth = 120;
    [Export] public int MaxArmor = 100;
    [Export] public Weapon[] Loadout;
}
