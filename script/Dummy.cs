using Godot;
using System;

public partial class Dummy : StaticBody3D, IDamagable
{
	[Export] public int Health = 100;
	[Export] public Weapon Weapon;

	private Node3D _firePoint;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_firePoint = GetNode<Node3D>("MeshInstance3D/FirePoint");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Damage(int damage, Vector3 knockback = new Vector3())
	{
		ReturnToFortress.LogInfo("Dummy took ", damage, " damage. Ow.");
		Health -= damage;
		if (Health <= 0) {
			ReturnToFortress.LogInfo("Dummy died. :(");
			QueueFree();
		}
	}
	private void _on_timer_timeout()
	{
		Weapon.Fire(GetParent(), _firePoint.GlobalTransform.Basis, _firePoint.GlobalPosition);
	}
}


