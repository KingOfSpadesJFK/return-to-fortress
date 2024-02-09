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
	public ENetMultiplayerPeer MultiplayerPeer { get; private set; }
	public int UniquePeerID { get; private set; }
	public Node MapRoot { get; private set; }
	public NodePath MapRootPath { get; private set; }
	public Node AreaRoot { get => MapRoot?.GetNode("Area"); }
	public NodePath AreaRootPath { get => MapRootPath.IsEmpty ? MapRootPath + "/Area" : ""; }
	public Node ActorRoot { get => MapRoot?.GetNode("Actor"); }
	public NodePath ActorRootPath { get => MapRootPath.IsEmpty ? MapRootPath + "/Actor" : ""; }
	public Node WorldRoot { get => MapRoot?.GetNode("World"); }
	public NodePath WorldRootPath { get => MapRootPath.IsEmpty ? MapRootPath + "/World" : ""; }
	public Random Random { get; private set; } = new Random();
	public Dictionary<int, PlayerInfo> Players { get; private set; } = new Dictionary<int, PlayerInfo>();
	public Player LocalPlayer { get; private set; }

	public const float SENSITIVITY_CONSTANT = 0.0075f;

	public override void _Ready()
	{
		ClientPlayerScene = GD.Load<PackedScene>("res://node/gordon.tscn");
		NetworkPlayerScene = GD.Load<PackedScene>("res://node/alyx.tscn");
		ProjectileScene = GD.Load<PackedScene>("res://node/projectile.tscn");
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

	public Error HostServer() 
	{
		MultiplayerPeer = new ENetMultiplayerPeer();
		if (MultiplayerPeer.CreateServer(ServerSettings.Port, 32) != Error.Ok) {
			LogError("Failed to create server.");
			return Error.Failed;
		}
		Multiplayer.MultiplayerPeer = MultiplayerPeer;
		UniquePeerID = Multiplayer.GetUniqueId();
		GotoMap("res://map/2fort.tscn");
		AddPlayer("Host", UniquePeerID);

		// Called once a peer is connected to the host
		MultiplayerPeer.PeerConnected += async (id) => {
			await ToSignal(GetTree().CreateTimer(0.1), Timer.SignalName.Timeout);
			if (	Rpc("AddConnectingPlayer", (int)id) != Error.Ok || 
					RpcId(id, "AddPreviouslyConnectedCharacters", Players.Keys.ToArray()) != Error.Ok) {
				LogError("Failed to send RPCs to peer ", id);
			} else {
				InstantiatePlayer(AddPlayer("Peer", (int)id));
				LogInfo("Peer connected with ID ", id);
			}
		};

		MultiplayerPeer.PeerDisconnected += (id) => {
			LogInfo("Peer disconnected with ID ", id);
			Players.Remove((int)id);
		};
		return Error.Ok;
	}

	public Error JoinServer() 
	{
		MultiplayerPeer = new ENetMultiplayerPeer();
		if (MultiplayerPeer.CreateClient(ServerSettings.IP, ServerSettings.Port) != Error.Ok) {
			LogError("Failed to connect to server.");
			return Error.Failed;
		}
		Multiplayer.MultiplayerPeer = MultiplayerPeer;
		UniquePeerID = Multiplayer.GetUniqueId();
		GotoMap("res://map/2fort.tscn");

		MultiplayerPeer.PeerDisconnected += (id) => {
			if (id == 1) {
				LogError("Disconnected from server.");
				LocalPlayer?.Free(); LocalPlayer = null;
				MapRoot?.Free(); MapRoot = null;
				Players.Clear();
			} else {
				LogInfo("Peer disconnected with ID ", id);
				Players.Remove((int)id);
			}
		};
		return Error.Ok;
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

	private void InstantiatePlayer(PlayerInfo infos)
	{
		var player = infos.ID == UniquePeerID ? ClientPlayerScene.Instantiate<Player>() : NetworkPlayerScene.Instantiate<Player>();
		player.Info = infos;
		player.Info.AddWeapon(GD.Load<Weapon>("res://resource/weapon/projectile/rocket_launcher.tres"));
		player.Info.AddWeapon(GD.Load<Weapon>("res://resource/weapon/hitscan/pistol.tres"));
		player.SetMultiplayerAuthority(player.Info.ID);
		player.Name = infos.Name + "#" + infos.ID;
		ActorRoot.AddChild(player);
		if (infos.ID == UniquePeerID) {
			LocalPlayer = player;
		}

		// TODO: Make it so players spawn in their team's spawn box
		foreach (var spawn in AreaRoot.GetChildren()) {
			if (spawn is SpawnBox spawnBox) {
				spawnBox.BringToSpawn(player);
				break;
			}
		}
	}

	private void InstantiatePlayers()
	{
		foreach (var info in Players.Values) {
			InstantiatePlayer(info);
		}
	}

	public void GotoMap(string mapPath)
	{
		CallDeferred(nameof(DeferredGotoMap), mapPath);
	}

	private void DeferredGotoMap(string mapPath)
	{
		MapRoot?.Free();
		var newMap = GD.Load<PackedScene>(mapPath);
		MapRoot = newMap.Instantiate();
		GetNode("/root/Control/SubViewportContainer/SubViewport").AddChild(MapRoot);
		MapRootPath = MapRoot.GetPath();
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
