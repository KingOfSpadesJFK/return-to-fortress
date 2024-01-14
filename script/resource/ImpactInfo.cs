using Godot;
using System;

[GlobalClass]
public partial class ImpactInfo : Resource
{
    [Export] public int Damage = 10;
	[Export] public float Knockback = 0.0f;
	private Resource StatusEffect = null;
	private Resource AreaOfEffect = null;

	public virtual void Impact(GodotObject obj) {
		ReturnToFortress.LogInfo("Impacted " + obj + "!");
		if (obj is IDamagable damagable) {
			damagable.Damage(Damage);
		}
	}
}
