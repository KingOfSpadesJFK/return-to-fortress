using Godot;
using System;

[GlobalClass]
public partial class Projectile : Node3D
{
	[Export] public ProjectileInfo Info;
	public float Speed = 50.0f;

	private MeshInstance3D _meshInstance;
	private GpuParticles3D _particles;
	private RayCast3D _rayCast;

	public override void _Ready()
	{
		_rayCast = GetNode<RayCast3D>("RayCast3D");
		if (Info is ProjectileInfo) {
			if (Info.ProjectileMesh is Mesh) {
				_meshInstance = new MeshInstance3D {
					Mesh = Info.ProjectileMesh,
					Skin = Info.ProjectileSkin
				};
				AddChild(_meshInstance);
			}
			if (Info.ParticleMesh is Mesh[])  {
				int length = Info.ParticleMesh.Length;
				_particles = new GpuParticles3D {
					DrawPasses = Info.ParticleMesh.Length,
					DrawSkin = Info.ParticleSkin
				};
				if (length > 0) _particles.DrawPass1 = Info.ParticleMesh[0];
				if (length > 1) _particles.DrawPass2 = Info.ParticleMesh[1];
				if (length > 2) _particles.DrawPass3 = Info.ParticleMesh[2];
				if (length > 3) _particles.DrawPass4 = Info.ParticleMesh[3];
				AddChild(_particles);
			}
		}
	}

	public override void _Process(double delta)
	{
		Position += Transform.Basis * new Vector3(0.0f, 0.0f, -Speed) * (float)delta;
		if (_rayCast.IsColliding()) {
			if (Info.Impaction is ImpactResource) Info.Impaction.Impact();
			QueueFree();
		}
	}
}
