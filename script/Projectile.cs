using Godot;
using System;

[GlobalClass]
public partial class Projectile : Node3D
{
	[Export] public ProjectileInfo Info;
	[Export] public float Speed = 50.0f;

	private MeshInstance3D _meshInstance;
	private GpuParticles3D _particles;
	private RayCast3D _rayCast;
	private Timer _timeoutTimer;
	private Timer _particleTimer;
	private bool _usesParticles = false;
	private bool _isDead = false;
	
	public override void _Ready()
	{
		_rayCast = GetNode<RayCast3D>("RayCast3D");

		// Create the timeout timer if the timeout timer is set.
		if (Info.TimeoutTimer > 0.0f) {
			_timeoutTimer = new Timer {
				WaitTime = Info.TimeoutTimer,
				OneShot = true
			};
			_timeoutTimer.Timeout += OnTimeout;
			AddChild(_timeoutTimer);
			_timeoutTimer.Start();
		}

		// Create the projectile mesh and particle effects if they are set.
		if (Info is ProjectileInfo) {
			// Create the projectile mesh instance.
			if (Info.ProjectileMesh is Mesh) {
				_meshInstance = new MeshInstance3D {
					Mesh = Info.ProjectileMesh,
					Skin = Info.ProjectileSkin
				};
				AddChild(_meshInstance);
			} else {
				// Because why would you want an invisible projectile?
				ReturnToFortress.LogError("Info.ProjectileMesh is not set!");
			}

			// Create the GPU particles instance.
			if (Info.ParticleMesh is Mesh[])  {
				int length = Info.ParticleMesh.Length;
				_particles = new GpuParticles3D {
					DrawPasses = Info.ParticleMesh.Length,
					DrawSkin = Info.ParticleSkin,
					ProcessMaterial = Info.ParticleMaterial,
					Lifetime = Info.ParticleLifetime
				};
				if (length > 0) _particles.DrawPass1 = Info.ParticleMesh[0];
				if (length > 1) _particles.DrawPass2 = Info.ParticleMesh[1];
				if (length > 2) _particles.DrawPass3 = Info.ParticleMesh[2];
				if (length > 3) _particles.DrawPass4 = Info.ParticleMesh[3];
				AddChild(_particles);
				_particles.Emitting = true;
				_usesParticles = true;

				// Create a timer to destroy the projectile after the projectile gets "destroyed".
				//  I tried hooking something to GpuParticle3D.Finished...
				//  Idk why it wasn't working.
				_particleTimer = new Timer {
					WaitTime = _particles.Lifetime,
					OneShot = true
				};
				_particleTimer.Timeout += OnParticleTimeout;
				AddChild(_particleTimer);
			}
		}
	}

	// Called when the particle timer expires.
	//  Kills the entire tree
	//  Only called when the projectile uses particles.
	//  Otherwise, the tree's killed on impact.
	private void OnParticleTimeout() {
		ReturnToFortress.LogInfo("Particle timeout");
		QueueFree();
	}

	// Called when the timeout timer expires. 
	//  This function will only be called when the timeout timer is set.
	//  Will QueueFree() the projectile no matter what.
	private void OnTimeout() {
		if (Info.TimeoutImpaction is ImpactResource) {
			ReturnToFortress.LogInfo("Projectile timeout at ", Position);
			Info.TimeoutImpaction.Impact();
		}
		Destroy();
	}
	
	private void Destroy() {
		if (_usesParticles){
			_meshInstance.Visible = false;
			_particles.OneShot = true;
			_particles.Emitting = false;
			_particleTimer.Start();
		} else {
			QueueFree();
		}
		_isDead = true;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	//  This function is used to move the projectile forward and check for collisions.
	//  Will QueueFree() the projectile if it collides with something and Impaction is set.
	public override void _Process(double delta)
	{
		if (!_isDead) {
			Position += Transform.Basis * new Vector3(0.0f, 0.0f, -Speed) * (float)delta;
			if (_rayCast.IsColliding() && Info.Impaction is ImpactResource) {
				Info.Impaction.Impact();
				Destroy();
			}
		}
	}
}
