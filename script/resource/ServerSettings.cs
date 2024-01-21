using Godot;
using System;

[GlobalClass]
public partial class ServerSettings : Resource
{
    [Export] public string ServerIP = "127.0.0.1";
    [Export] public int Port = 42069;
    [Export] public int TickRate = 60;
    [Export] public string Name = "Return to Fortress";
    [Export] public string Description = "This is a server description.";
    [Export] public string Password = "";
    [Export] public bool IsPublic = true;
    [Export] public int MaxPlayers = 24;
    [Export] public Team[] Teams;
}
