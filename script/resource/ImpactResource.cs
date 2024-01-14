using Godot;
using System;

[GlobalClass]
public partial class ImpactResource : Resource
{
    [Export] public int Damage = 10;
	[Export] public float Knockback = 0.0f;
	private Resource StatusEffect = null;
	private Resource AreaOfEffect = null;

	public virtual void Impact() {
		ReturnToFortress.LogInfo("Impacted!");
	}
}
