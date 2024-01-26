using System;
using Godot;

public enum PlayerState {
	Idle,
	Walking,
	Braking,
	CrouchIdle,
	CrouchWalking,
	CrouchBraking,
	Jumping,
	Falling,
	Dead
}

[GlobalClass]
public partial class Player : CharacterBody3D, IDamagable
{
	[Export] public PlayerInfo Info { get; private set; }
	[Export] private float _walkingAcceleration = 35.0f;
	[Export] private float _walkingSpeedCap = 5.0f;
	[Export] private float _walkingDeceleration = 7.0f;
	[Export] private float _walkingAirAcceleration = 20.0f;
	[Export] private float _airResistance = 0.1250f;
	[Export] private float _jumpVelocity = 7.50f;
	[Export] private float _jumpForwardVelocity = 1.0f;
	[Export] private float _gravity = 20.0f;				// The player gravity should be independent of the map gravity.
	[Export] private int _maxJumps = 1;
	[Export] private Node3D _firePoint;
	[Export] private bool _ignoreClientInput = false;		// Set to true if this player is controlled over the network or by an AI.
	public Weapon Weapon { get => Info.Weapon; }

	[Signal] public delegate void PlayerJumpEventHandler();
	[Signal] public delegate void PlayerFireEventHandler();
	[Signal] public delegate void PlayerReloadEventHandler();
	[Signal] public delegate void PlayerSwapEventHandler(int weaponIndex);
	[Signal] public delegate void PlayerCrouchEventHandler(bool crouched);
	[Signal] public delegate void PlayerDamageEventHandler();
	[Signal] public delegate void PlayerDeathEventHandler();

	private Projectile _projectileInstance;
	private Node3D _head;
	private Node3D _eye;

	private int _jumps = 0;
	private bool _isJumping = false;
	private bool _isCrouching = false;
	private bool _isFiring = false;
	private Vector3 _velocity;
	private Vector2 _velocityXZ {
		get => new Vector2(_velocity.X, _velocity.Z);
		set => _velocity = new Vector3(value.X, _velocity.Y, value.Y);
	}

	private float _velocityY {
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
		_eye = GetNode<Node3D>("Head/Eye");
		_velocity = Velocity;

		if (!_ignoreClientInput) Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public void Damage(int damage, Vector3 knockback, PlayerInfo playerInfo = null) {
		Info.Health -= damage;
		_velocity += knockback;
		ReturnToFortress.LogInfo(Info.Name + "<" + Info.ID + "> health: ", Info.Health);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		// Mouse movement
		if (!_ignoreClientInput && @event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured) {
			Vector2 mouseVelocity = mouseMotion.Relative * ReturnToFortress.Instance.ClientSettings.MouseSensitivity * ReturnToFortress.SENSITIVITY_CONSTANT;
			_head.RotateY(-mouseVelocity.X);
			_eye.RotateX(-mouseVelocity.Y);
			_eye.RotationDegrees = new Vector3(
				Mathf.Clamp(_eye.RotationDegrees.X, -90, 90),
				_eye.RotationDegrees.Y,
				_eye.RotationDegrees.Z
			);
		}
	}

	// Handle single press actions
	public override void _Input(InputEvent @event)
	{
		if (!_ignoreClientInput) {
			// Add a force to the player in the direction they are looking
			//  This is for debugging purposes only
			if (@event.IsActionPressed("debug_playerimpulse")) {
				Vector3 headDir = (_head.Transform.Basis * new Vector3(0, 0, -1)).Normalized();
				_velocityXZ += new Vector2(headDir.X, headDir.Z) * 50.0f;
			}

			// Jump
			if (@event.IsActionPressed("player_movement_jump") && _jumps < _maxJumps) {
				HandleJump();
				_jumps++;
			}

			// Weapon swap
			if (@event.IsActionPressed("player_weapon_prev")) {
				Info.PreviousWeapon();
			}

			if (@event.IsActionPressed("player_weapon_next")) {
				Info.NextWeapon();
			}
		}
	}

	// Handle polling actions
	public override void _Process(double delta)
	{
		if (!_ignoreClientInput) {
			// Get the wish direction from the input
			Vector2 inputDir = Input.GetVector("player_movement_left", "player_movement_right", "player_movement_up", "player_movement_down");
			_wishDir = (_head.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

			// Fire
			if (Input.IsActionJustPressed("player_fire") && Input.MouseMode == Input.MouseModeEnum.Captured) {
				if (Weapon is not null) {
					Weapon.Fire(this, _firePoint.GlobalTransform.Basis, _firePoint.GlobalPosition);
					_isFiring = true;
				}
			}
		}
		
	}

	// Handle physics processing
	public override void _PhysicsProcess(double delta)
	{
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

		// Handle movement
		if (_wishDir != Vector3.Zero) {
			HandleMovement(delta);
		}

		Velocity = _velocity;
		MoveAndSlide();

		_isJumping = false;
		_isFiring = false;
		_isCrouching = false;
	}

	private void StorePlayerInfo() {
		Info.Velocity = Velocity;
		Info.Position = Position;
		Info.Rotation = Rotation;
		Info.HeadRotation = _head.Rotation;
		Info.EyeRotation = _eye.Rotation;
	}

	private void HandleMidAir(double delta) {
		// Handle gravity
		_velocity.Y -= _gravity * (float)delta;
	}

	private void HandleGround(double delta) {
		// Handle friction
		_jumpDirection = Vector2.Zero;
		_velocityXZ = _velocityXZ.Lerp(Vector2.Zero, _walkingDeceleration * (float)delta);
	}

	private void HandleJump() {
		_velocity.Y = _jumpVelocity;
		_jumpDirection = _wishDirXZ;
		if (IsOnFloor()) {
			// Add the jump forward velocity to the current velocity on the ground
			_velocityXZ += _wishDirXZ * _jumpForwardVelocity;
		} else {
			// Set the velocity to the walking speed cap + the jump forward velocity in mid air
			_velocityXZ = _wishDirXZ * _walkingSpeedCap;
		}
		_jumps++;
		_isJumping = true;
	}

	private void HandleMovement(double delta) {
		// Handle acceleration
		//  Only if the player is moving slower than the speed cap or holding any direction
		//  other than forward.
		if (_velocityXZ.Length() < _walkingSpeedCap + _jumpForwardVelocity || 
				_velocityXZ.Normalized().Dot(_wishDirXZ) <= 0.5f) {
			// jumpDirDot is the dot product of the velocity direction and the wish direction
			//  It lets the player move backwards in the air after jumping in a direction
			float jumpDirDot = _jumpDirection != Vector2.Zero ? 
				Mathf.Clamp((-_velocityXZ.Normalized().Dot(_wishDirXZ) + 1.0f) / 2.0f, 0.375f, 1.0f) : 0.375f;
			// Different accelearation rates in the air and on the ground
			//  Mid air acceleration is affected by jumpDirDot^2
			float acceleration = IsOnFloor() ? _walkingAcceleration : _walkingAirAcceleration * jumpDirDot;
			_velocityXZ += _wishDirXZ * acceleration * (float)delta;
		}
	}
}
