using System;
using Godot;

public partial class Player : CharacterBody3D
{
	[Export] public PlayerInfo Info;
	[Export] public float WalkingAcceleration = 7.50f;
	[Export] public float WalkingSpeedCap = 10.0f;
	[Export] public float WalkingDeceleration = 6.750f;
	[Export] public float WalkingAirAcceleration = 3.0f;
	[Export] public float AirResistance = 0.1250f;
	[Export] public float JumpVelocity = 7.50f;
	[Export] public float JumpForwardVelocity = 1.0f;
	[Export] public float Gravity = 20.0f;				// The player gravity should be independent of the map gravity.
	[Export] public int MaxJumps = 1;
	public float Sensitivity = 0.750f;

	private const float SENSITIVITY_CONSTANT = 0.0075f;
	private Projectile _projectileInstance;

	private Node3D _head;
	private Camera3D _eye;
	private Node3D _firePoint;

	private int _jumps = 0;
	private Vector3 _velocity;
	private Vector2 _velocityXZ {
		get => new Vector2(_velocity.X, _velocity.Z);
		set => _velocity = new Vector3(value.X, _velocity.Y, value.Y);
	}

	private float 	_velocityY {
		get => _velocity.Y;
		set => _velocity = new Vector3(_velocity.X, value, _velocity.Z);
	}

	private Vector3 _wishDir;
	private Vector2 _wishDirXZ {
		get => new Vector2(_wishDir.X, _wishDir.Z);
		set => _wishDir = new Vector3(value.X, _wishDir.Y, value.Y);
	}
	private Vector2 _jumpDirection;

	public override void _Ready()
	{
		_head = GetNode<Node3D>("Head");
		_eye = GetNode<Camera3D>("Head/Eye");
		_firePoint = GetNode<Node3D>("Head/Eye/FirePoint");

		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		// Mouse movement
		if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured) {
			_head.RotateY(-mouseMotion.Relative.X * Sensitivity * SENSITIVITY_CONSTANT);
			_eye.RotateX(-mouseMotion.Relative.Y * Sensitivity * SENSITIVITY_CONSTANT);
			_eye.RotationDegrees = new Vector3(
				Mathf.Clamp(_eye.RotationDegrees.X, -90, 90),
				_eye.RotationDegrees.Y,
				_eye.RotationDegrees.Z
			);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		_velocity = Velocity;
		Vector2 inputDir = Input.GetVector("player_movement_left", "player_movement_right", "player_movement_up", "player_movement_down");
		_wishDir = (_head.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		if (Input.IsActionJustPressed("player_fire")) {
			// _projectileInstance = projectile.Instantiate<Projectile>();
			// _projectileInstance.Position = _head.GlobalPosition;
			// _projectileInstance.Transform = _head.Transform;
			if (Info.Weapon is Weapon weapon) {
				weapon.Fire(GetParent(), _firePoint.GlobalTransform.Basis, _firePoint.GlobalPosition);
			}
		}

		// Add a force to the player in the direction they are looking
		//  This is for debugging purposes only
		if (Input.IsActionJustPressed("debug_playerimpulse")) {
			Vector3 headDir = (_head.Transform.Basis * new Vector3(0, 0, -1)).Normalized();
			_velocityXZ += new Vector2(headDir.X, headDir.Z) * 50.0f;
		}

		// Handle mid air and ground
		if (!IsOnFloor()) {
			HandleMidAir(delta);
			if (_jumps == 0) {
				_jumps++;
			}
		} else {
			HandleGround(delta);
			_jumps = 0;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("player_movement_jump") && _jumps < MaxJumps) {
			HandleJump(delta);
			_jumps++;
		}

		// Handle movement
		if (_wishDir != Vector3.Zero) {
			HandleMovement(delta);
		}

		Velocity = new Vector3(_velocityXZ.X, _velocity.Y, _velocityXZ.Y);
		MoveAndSlide();
	}

	private void HandleMidAir(double delta) {
		// Handle gravity
		_velocity.Y -= Gravity * (float)delta;
	}

	private void HandleGround(double delta) {
		// Handle friction
		_velocityXZ = _velocityXZ.Lerp(Vector2.Zero, WalkingDeceleration * (float)delta);
	}

	private void HandleJump(double delta) {
		_velocity.Y = JumpVelocity;
		_jumpDirection = _wishDirXZ;
		if (IsOnFloor()) {
			// Add the jump forward velocity to the current velocity on the ground
			_velocityXZ += _wishDirXZ * JumpForwardVelocity;
		} else {
			// Set the velocity to the walking speed cap + the jump forward velocity in mid air
			_velocityXZ = _wishDirXZ * WalkingSpeedCap;
		}
	}

	private void HandleMovement(double delta) {
		// Handle acceleration
		//  Only if the player is moving slower than the speed cap
		if (_velocityXZ.Length() < WalkingSpeedCap + JumpForwardVelocity) {
			// jumpDirDot is the dot product of the velocity direction and the wish direction
			//  It lets the player move backwards in the air after jumping in a direction
			float jumpDirDot = !IsOnFloor() && _velocityXZ != Vector2.Zero && _jumpDirection != Vector2.Zero ? 
				Mathf.Clamp((-_velocityXZ.Dot(_wishDirXZ) + 1.0f) / 2.0f, 0.5f, 1.0f) : 0.5f;
			// Different accelearation rates in the air and on the ground
			//  Mid air acceleration is affected by jumpDirDot^2
			float acceleration = IsOnFloor() ? WalkingAcceleration : WalkingAirAcceleration * jumpDirDot * jumpDirDot;
			_velocityXZ = _velocityXZ.Lerp(_wishDirXZ * WalkingSpeedCap, acceleration * (float)delta);
		}
	}
}
