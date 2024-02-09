using Godot;
using System;

[GlobalClass]
public partial class ServerSettings : Resource
{
    [Export] public string IP = "127.0.0.1";      // localhost
    [Export] public int Port = 8000;
    [Export] public int TickRate = 60;
    [Export] public string Name = "Return to Fortress";
    [Export] public string Description = "This is a server description.";
    [Export] public string Password = "";
    [Export] public bool IsPublic = true;
    [Export] public int MaxPlayers = 24;
    [Export] public float RespawnTime = 5.0f;
    [Export] public Team[] Teams;
}
