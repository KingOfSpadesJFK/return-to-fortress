using Godot;
using System;
using System.Diagnostics;
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

	public const float SENSITIVITY_CONSTANT = 0.0075f;

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

		// Spawn players
		var clientPlayer = ClientPlayerScene.Instantiate<Player>();
		CurrentMap.GetNode("Actor").AddChild(clientPlayer);
		CurrentMap.GetNode<SpawnBox>("Area/RedSpawn").SpawnPlayer(clientPlayer);
	}

	public static void LogInfo(params object[] what)
	{
		var methodInfo = new StackTrace().GetFrame(1).GetMethod();
		var className = methodInfo.ReflectedType.Name;
		GD.Print("[INFO] ", className, ": ", BuildParamStrings(what));
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
		GD.PrintErr("[ERROR] ", className, ": ", BuildParamStrings(what));
	}
}
