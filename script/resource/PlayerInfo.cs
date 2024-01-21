using Godot;
using System;

[GlobalClass]
public partial class PlayerInfo : Resource
{
	[Export] public string ID = "0";
	[Export] public string Name = "Player";
	[Export] public PlayerClass Class;
	[Export] public int Health = 100;
	[Export] public int Armor = 0;
	[Export] public int Kills = 0;
	[Export] public int Deaths = 0;
	[Export] public int Assists = 0;
	[Export] public int Score = 0;
	[Export] public int Ping = 0;
	[Export] public Team Team;
	[Export] public Godot.Collections.Array<Weapon> Loadout;
	[Export] public Vector3 Position;
	[Export] public Vector3 Rotation;
	[Export] public Vector3 WishDirection;
	[Export] public Vector3 Velocity;
	[Export] public Vector3 HeadPosition;
	[Export] public Vector3 HeadRotation;
	[Export] public Vector3 EyeRotation;

	public PlayerState PlayerState;

	public Weapon Weapon { get => Loadout[_weaponIndex]; }
	private int _weaponIndex = 0;

	public PlayerInfo() : this(null, null, new PlayerClass()) {}

	public PlayerInfo(string id, string name, PlayerClass playerClass) {
		ID = id;
		Name = name;
		Class = playerClass;
		Health = playerClass.MaxHealth;
		Armor = playerClass.MaxArmor;
	}

	public void NextWeapon() {
		_weaponIndex = (_weaponIndex + 1) % Loadout.Count;
	}

	public void PreviousWeapon() {
		_weaponIndex = (_weaponIndex - 1) % Loadout.Count;
		if (_weaponIndex < 0) _weaponIndex = Loadout.Count - 1;
	}

	public void AddWeapon(Weapon weapon) {
		Loadout.Add(weapon);
	}

	public void RemoveWeapon(Weapon weapon) {
		Loadout.Remove(weapon);
	}

	public void SetWeapon(int index) {
		_weaponIndex = index;
	}
}
