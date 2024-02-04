using Godot;
using System;

[GlobalClass]
public partial class PlayerInfo : Resource
{
	[Export] public int ID = 0;
	[Export] public string Name = "Player";
	[Export] public PlayerClass Class;
	[Export] public Team Team;
	[Export] public Godot.Collections.Array<Weapon> Loadout = new Godot.Collections.Array<Weapon>();
	public int MaxHealth { get => Class.MaxHealth; }
	public int MaxArmor { get => Class.MaxArmor; }
	public int Health = 100;
	public int Armor = 0;
	public int Kills = 0;
	public int Deaths = 0;
	public int Assists = 0;
	public int Score = 0;
	public int Ping = 0;
	public Vector3 Position;
	public Vector3 Rotation;
	public Vector3 WishDirection;
	public Vector3 Velocity;
	public Vector3 HeadPosition;
	public Vector3 HeadRotation;
	public Vector3 EyeRotation;

	public PlayerState PlayerState;

	public Weapon Weapon { get => Loadout[WeaponIndex]; }
	public int WeaponIndex = 0;

	public PlayerInfo() : this(0, null, new PlayerClass()) {}

	public PlayerInfo(int id, string name, PlayerClass playerClass) {
		ID = id;
		Name = name;
		Class = playerClass;
		Health = playerClass.MaxHealth;
		Armor = playerClass.MaxArmor;
	}

	public void NextWeapon() {
		WeaponIndex = (WeaponIndex + 1) % Loadout.Count;
	}

	public void PreviousWeapon() {
		WeaponIndex = (WeaponIndex - 1) % Loadout.Count;
		if (WeaponIndex < 0) WeaponIndex = Loadout.Count - 1;
	}

	public void AddWeapon(Weapon weapon) {
		Loadout.Add(weapon);
	}

	public void RemoveWeapon(Weapon weapon) {
		Loadout.Remove(weapon);
	}

	public void SetWeapon(int index) {
		WeaponIndex = index;
	}
}
