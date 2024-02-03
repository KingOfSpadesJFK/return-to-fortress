using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;

// This class functions as a singleton, and is used to store packed scenes, client settings,
//  and global functions used throughout the game. This class should NOT hold player data and
//  instead should be used as a means for loading maps and other resources, networking, or
//  debugging.
public partial class ReturnToFortress : Node
{
	public static ReturnToFortress Instance { get; private set; }

	public PackedScene ClientPlayerScene { get; private set; }
	public PackedScene NetworkPlayerScene { get; private set; }
	public PackedScene ProjectileScene { get; private set; }
	public ClientSettings ClientSettings { get; private set; } = new ClientSettings();
	public ServerSettings ServerSettings { get; private set; } = new ServerSettings();
	public ENetMultiplayerPeer ClientPeer { get; private set; }
	public ENetMultiplayerPeer ServerPeer { get; private set; }
	public Node CurrentMap { get; private set; }
	public Node MapRoot { get; set; }
	public Random Random { get; private set; } = new Random();
	public Dictionary<int, PlayerInfo> Players { get; private set; } = new Dictionary<int, PlayerInfo>();

	public const float SENSITIVITY_CONSTANT = 0.0075f;
	private const int PORT = 8200;
	private const string IP = "127.0.0.1";		// localhost
	public int UniquePeerID { get; private set; }

	public override void _Ready()
	{
		ClientPlayerScene = GD.Load<PackedScene>("res://node/gordon.tscn");
		NetworkPlayerScene = GD.Load<PackedScene>("res://node/alyx.tscn");
		ProjectileScene = GD.Load<PackedScene>("res://node/projectile.tscn");
		MapRoot = GetNode("/root/Control/SubViewportContainer/MapRoot");
		if (Instance is null) {
			Instance = this;
		} else {
			LogError("ReturnToFortress instance already exists!");
			throw new Exception("ReturnToFortress instance already exists!");
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void HostServer() 
	{
		ServerPeer = new ENetMultiplayerPeer();
		ServerPeer.CreateServer(PORT, 32);
		Multiplayer.MultiplayerPeer = ServerPeer;
		UniquePeerID = Multiplayer.GetUniqueId();
		GotoMap("res://map/2fort.tscn");
		AddPlayer("Host", UniquePeerID);

		// Called once a peer is connected to the host
		ServerPeer.PeerConnected += async (id) => {
			await ToSignal(GetTree().CreateTimer(1.0), Timer.SignalName.Timeout);
			Rpc("AddConnectingPlayer", (int)id);
			RpcId(id, "AddPreviouslyConnectedCharacters", Players.Keys.ToArray());
			InstantiatePlayer(AddPlayer("Peer", (int)id));
			LogInfo("Peer connected with ID ", id);
			foreach (var i in Players.Keys) {
				LogInfo(i);
			}
		};
	}

	private void InstantiatePlayer(PlayerInfo infos)
	{
		var player = infos.ID == UniquePeerID ? ClientPlayerScene.Instantiate<Player>() : NetworkPlayerScene.Instantiate<Player>();
		player.Info = infos;
		player.SetMultiplayerAuthority(player.Info.ID);
		CurrentMap.GetNode("Actor").AddChild(player);
	}

	private void InstantiatePlayers()
	{
		foreach (var info in Players.Values) {
			InstantiatePlayer(info);
		}
	}

	public void JoinServer() 
	{
		ClientPeer = new ENetMultiplayerPeer();
		ClientPeer.CreateClient(IP, PORT);
		Multiplayer.MultiplayerPeer = ClientPeer;
		UniquePeerID = Multiplayer.GetUniqueId();
		GotoMap("res://map/2fort.tscn");
	}

	[Rpc]
	private void AddConnectingPlayer(int id) 
	{
		InstantiatePlayer(AddPlayer("Peer", id));
	}

	[Rpc]
	private void AddPreviouslyConnectedCharacters(int[] infos)
	{
		LogInfo("Adding previously connected characters");
		foreach (var info in infos) {
			// LogInfo("Adding ", info.Name, " with ID ", info.ID, " to Players list.");
			var pInfo = AddPlayer(info == 1 ? "Host" : "Peer", info);
			InstantiatePlayer(pInfo);
		}
	}

	public PlayerInfo AddPlayer(string name, int id)
	{
		PlayerInfo player = new PlayerInfo(id, name, new PlayerClass());
		player.Name = name;
		player.ID = id;
		Players.Add(id, player);
		return player;
	}

	public void GotoMap(string mapPath)
	{
		CallDeferred(nameof(DeferredGotoMap), mapPath);
	}

	private void DeferredGotoMap(string mapPath)
	{
		CurrentMap?.Free();
		var newMap = GD.Load<PackedScene>(mapPath);
		CurrentMap = newMap.Instantiate();
		MapRoot.AddChild(CurrentMap);
		InstantiatePlayers();
	}

	public static void LogInfo(params object[] what)
	{
		var methodInfo = new StackTrace().GetFrame(1).GetMethod();
		var className = methodInfo.ReflectedType.Name;
		GD.Print("[INFO](" + Instance.UniquePeerID + ") ", className, ": ", BuildParamStrings(what));
	}

	private static string BuildParamStrings(params object[] what) 
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (what == null || what.Length == 0) {
			stringBuilder.Append("null");
		} else {
			for (int i = 0; i < what.Length; i++) {
				stringBuilder.Append(what[i]?.ToString() ?? "null");
			}
		}
		return stringBuilder.ToString();
	}

	public static void LogError(params object[] what)
	{
		var methodInfo = new StackTrace().GetFrame(1).GetMethod();
		var className = methodInfo.ReflectedType.Name;
		GD.PrintErr("[ERROR](" + Instance.UniquePeerID + ") ", className, ": ", BuildParamStrings(what));
	}
}
