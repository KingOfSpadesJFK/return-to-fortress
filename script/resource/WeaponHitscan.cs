using Godot;
using Godot.Collections;
using System;

[GlobalClass]
public partial class WeaponHitscan : Weapon
{
	[Export] public float Range = 100.0f;
    [Export] public float Spread = 0.0f;
    [Export] public int Shrapnel = 1;
    [Export] public ImpactInfo Impaction;

	public override void Fire(Node3D source, Basis initBasis, Vector3 initPos)
	{
		var spaceState = source.GetWorld3D().DirectSpaceState;
		var from = initPos + FirePointOffset;
		var to = from + -initBasis.Z * Range;
		var query = PhysicsRayQueryParameters3D.Create(from, to);
		query.CollideWithAreas = true;
        query.CollideWithBodies = true;

		var result = spaceState.IntersectRay(query);
		if (result.Count > 0) {
            var player = source as Player;
            Impaction.Impact(result["collider"].AsGodotObject(), player?.Info);
		}
	}
}
