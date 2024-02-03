using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class Team : Resource
{
	[Export] public string Name = "Red";
	[Export] public Color Color = new Color(1, 0, 0);
	[Export] public int Score = 0;
	[Export] public int MaxPlayers = 0;

	public Team() : this(null, new Color(0, 0, 0), 0) {}

	public Team(string name, Color color, int maxPlayers) {
		Name = name;
		Color = color;
		maxPlayers = MaxPlayers;
	}
}
