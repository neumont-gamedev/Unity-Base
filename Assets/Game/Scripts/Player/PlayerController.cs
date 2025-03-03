using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player movement, input processing, and physics interactions.
/// Manages player state, animations, and responds to game events.
/// Requires a CharacterController component to function.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	/// <summary>
	/// ScriptableObject containing all player movement and physics parameters.
	/// Centralizes tuning values for jump height, movement speed, etc.
	/// </summary>
	[SerializeField, Tooltip("Reference to player movement and physics settings")]
	PlayerData playerData;

	/// <summary>
	/// The camera or view transform that determines movement direction.
	/// Used to convert input direction to world-space movement.
	/// </summary>
	[SerializeField, Tooltip("Camera or view transform that determines movement direction")]
	Transform view;

	/// <summary>
	/// Reference to the player's animator component for controlling animations.
	/// Updates animation states based on movement, jumping, and combat actions.
	/// </summary>
	[SerializeField, Tooltip("The animator component for controlling player animations")]
	Animator animator;

	/// <summary>
	/// ScriptableObject that stores and broadcasts the player's current health.
	/// Used for UI updates and game state management.
	/// </summary>
	[SerializeField, Tooltip("Stores and broadcasts player health as a percentage")]
	FloatData healthData;

	/// <summary>
	/// ScriptableObject that stores and broadcasts the player's current score.
	/// Used for UI updates and game progression.
	/// </summary>
	[SerializeField, Tooltip("Stores and broadcasts player score")]
	IntData scoreData;

	/// <summary>
	/// Event raised when the game is paused or unpaused.
	/// Triggers pause menu and game state changes.
	/// </summary>
	[SerializeField, Tooltip("Event raised when the pause button is pressed")]
	Event onPauseEvent;

	/// <summary>
	/// ScriptableObject that stores the current pause state.
	/// Used to synchronize pause state across different systems.
	/// </summary>
	[SerializeField, Tooltip("Stores the current pause state")]
	BoolData pauseData;

	/// <summary>
	/// Event raised when the player dies.
	/// Triggers game over sequence and cleanup.
	/// </summary>
	[SerializeField, Tooltip("Event raised when player health reaches zero")]
	GameObjectEvent onPlayerDeathEvent;

	/// <summary>
	/// The CharacterController component used for movement and collision.
	/// Handles physics-based movement and ground detection.
	/// </summary>
	CharacterController controller;

	// Input action references from the Input System
	/// <summary>Input action for player movement</summary>
	InputAction moveAction;
	/// <summary>Input action for jumping</summary>
	InputAction jumpAction;
	/// <summary>Input action for sprinting</summary>
	InputAction sprintAction;
	/// <summary>Input action for attacking</summary>
	InputAction attackAction;
	/// <summary>Input action for pausing the game</summary>
	InputAction pauseAction;

	// Current state variables
	/// <summary>
	/// Raw input values for movement (x = horizontal, y = vertical).
	/// Used to calculate movement direction.
	/// </summary>
	Vector2 movementInput = Vector2.zero;

	/// <summary>
	/// Current velocity vector of the player.
	/// Tracks movement in all three axes including gravity.
	/// </summary>
	Vector3 velocity = Vector3.zero;

	/// <summary>
	/// Whether the player is currently sprinting.
	/// Affects movement speed calculations.
	/// </summary>
	bool isSprinting = false;

	/// <summary>
	/// Public accessor for the sprinting state.
	/// Allows other components to check if player is sprinting.
	/// </summary>
	public bool IsSprinting => isSprinting;

	/// <summary>
	/// Public accessor for the view transform.
	/// Allows runtime modification of the camera reference.
	/// </summary>
	public Transform View { get => view; set => view = value; }

	/// <summary>
	/// Initialize references and input actions.
	/// Called when the script instance is first loaded.
	/// </summary>
	void Awake()
	{
		// Get the CharacterController component
		controller = GetComponent<CharacterController>();

		if (TryGetComponent(out Destructable destructable))
		{
			healthData.Value = destructable.CurrentHealth / destructable.MaxHealth;
		}

		// Find all required input actions
		moveAction = InputSystem.actions.FindAction("Move");
		jumpAction = InputSystem.actions.FindAction("Jump");
		sprintAction = InputSystem.actions.FindAction("Sprint");
		attackAction = InputSystem.actions.FindAction("Attack");
		pauseAction = InputSystem.actions.FindAction("Pause");
	}

	/// <summary>
	/// Subscribe to input events and enable input actions when this component is enabled.
	/// Sets up all input callbacks and ensures input system is active.
	/// </summary>
	private void OnEnable()
	{
		// Subscribe to events
		moveAction.performed += OnMove;
		moveAction.canceled += OnMove;

		jumpAction.performed += OnJump;
		jumpAction.canceled += OnJump;

		sprintAction.performed += OnSprint;
		sprintAction.canceled += OnSprint;

		attackAction.performed += OnAttack;
		attackAction.canceled += OnAttack;

		pauseAction.performed += OnPause;
		pauseAction.canceled += OnPause;

		// Enable actions if needed
		moveAction.Enable();
		jumpAction.Enable();
		sprintAction.Enable();
		attackAction.Enable();
		pauseAction.Enable();
	}

	/// <summary>
	/// Unsubscribe from input events and disable input actions when this component is disabled.
	/// Cleans up input callbacks and prevents input processing when inactive.
	/// </summary>
	private void OnDisable()
	{
		// Unsubscribe from events
		moveAction.performed -= OnMove;
		moveAction.canceled -= OnMove;

		jumpAction.performed -= OnJump;
		jumpAction.canceled -= OnJump;

		sprintAction.performed -= OnSprint;
		sprintAction.canceled -= OnSprint;

		attackAction.performed -= OnAttack;
		attackAction.canceled -= OnAttack;

		pauseAction.performed -= OnPause;
		pauseAction.canceled -= OnPause;

		// Disable actions if needed
		moveAction.Disable();
		jumpAction.Disable();
		sprintAction.Disable();
		attackAction.Disable();
		pauseAction.Disable();
	}

	/// <summary>
	/// Handle player movement, physics, and animation updates each frame.
	/// Core gameplay loop that processes input and updates player state.
	/// </summary>
	private void Update()
	{
		// Check if the player is on ground
		bool onGround = controller.isGrounded;// || (velocity.y < 0 && PredictGroundContact());

		// Reset vertical velocity when grounded to prevent accumulating downward force
		if (onGround && velocity.y < 0)
		{
			velocity.y = -1; // Small downward force to keep player grounded
		}

		// Convert movement input into a world-space direction based on the player's view rotation
		Vector3 movement = new Vector3(movementInput.x, 0, movementInput.y);
		movement = Quaternion.AngleAxis(view.rotation.eulerAngles.y, Vector3.up) * movement;

		// Initialize acceleration vector for movement calculations
		Vector3 acceleration = Vector3.zero;
		acceleration.x = movement.x * playerData.acceleration;
		acceleration.z = movement.z * playerData.acceleration;

		// Reduce acceleration while in the air for smoother movement control
		if (!onGround) acceleration *= 0.1f;

		// Extract horizontal velocity (ignoring vertical movement)
		Vector3 vXZ = new Vector3(velocity.x, 0, velocity.z);

		// Apply acceleration to velocity while limiting max speed
		vXZ += acceleration * Time.deltaTime;
		vXZ = Vector3.ClampMagnitude(vXZ, (isSprinting) ? playerData.sprintSpeed : playerData.speed);

		// Assign updated velocity values
		velocity.x = vXZ.x;
		velocity.z = vXZ.z;

		// Apply drag to slow the player down when there is no input or when airborne
		if (movement.sqrMagnitude <= 0 || !onGround)
		{
			float drag = (onGround) ? playerData.deceleration : playerData.decelerationInAir;
			velocity.x = Mathf.MoveTowards(velocity.x, 0, drag * Time.deltaTime);
			velocity.z = Mathf.MoveTowards(velocity.z, 0, drag * Time.deltaTime);
		}

		// Smoothly rotate the player towards the movement direction
		if (movement.sqrMagnitude > 0)
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), Time.deltaTime * playerData.turnRate);
		}

		// Apply gravity
		velocity.y += playerData.gravity * Time.deltaTime;

		// Move the character using the CharacterController component
		controller.Move(velocity * Time.deltaTime);

		// Update animator parameters for movement animations
		if (animator != null) animator.SetFloat("Speed", new Vector3(velocity.x, 0, velocity.z).magnitude);
		if (animator != null) animator.SetFloat("AirSpeed", controller.velocity.y);
		if (animator != null) animator.SetBool("OnGround", onGround);
	}

	#region Input Actions
	/// <summary>
	/// Callback for the move input action.
	/// Updates movement input vector based on player's directional input.
	/// </summary>
	/// <param name="ctx">Input action context containing movement values</param>
	public void OnMove(InputAction.CallbackContext ctx)
	{
		movementInput = ctx.ReadValue<Vector2>();
	}

	/// <summary>
	/// Callback for the jump input action.
	/// Applies vertical velocity for jumping when player is grounded.
	/// </summary>
	/// <param name="ctx">Input action context</param>
	public void OnJump(InputAction.CallbackContext ctx)
	{
		if (ctx.phase == InputActionPhase.Performed && controller.isGrounded)
		{
			// Calculate jump velocity based on desired jump height and gravity
			// This formula is derived from the physics equation: v² = v?² + 2a(?y)
			// Rearranged for initial velocity: v? = ?(2g·h) where:
			//   g = gravity (negative value for downward force)
			//   h = desired jump height
			// The negative sign before 2 compensates for the negative gravity value
			velocity.y = Mathf.Sqrt(-2 * playerData.gravity * playerData.jumpHeight);
			if (animator != null) animator.SetTrigger("Jump");
		}
	}

	/// <summary>
	/// Callback for the sprint input action.
	/// Toggles sprinting state based on button press/release.
	/// </summary>
	/// <param name="ctx">Input action context</param>
	public void OnSprint(InputAction.CallbackContext ctx)
	{
		if (ctx.phase == InputActionPhase.Performed) isSprinting = true;
		else if (ctx.phase == InputActionPhase.Canceled) isSprinting = false;
	}

	/// <summary>
	/// Callback for the attack input action.
	/// Triggers attack animation and potential combat logic.
	/// </summary>
	/// <param name="ctx">Input action context</param>
	public void OnAttack(InputAction.CallbackContext ctx)
	{
		// Prevent attacking when clicking on UI elements
		//if (EventSystem.current.IsPointerOverGameObject()) return;

		if (ctx.phase == InputActionPhase.Performed && animator != null) animator.SetTrigger("Attack");
	}

	/// <summary>
	/// Callback for the pause input action.
	/// Toggles game pause state and raises the pause event.
	/// </summary>
	/// <param name="ctx">Input action context</param>
	public void OnPause(InputAction.CallbackContext ctx)
	{
		if (ctx.phase == InputActionPhase.Performed)
		{
			pauseData.Value = !pauseData.Value;
			onPauseEvent.RaiseEvent();
		}
	}

	#endregion

	/// <summary>
	/// Called when the player takes damage.
	/// Updates health data based on current health of the Destructable component.
	/// </summary>
	/// <param name="damageInfo">Information about the damage received</param>
	public void OnDamaged(DamageInfo damageInfo)
	{
		if (TryGetComponent(out Destructable destructable))
		{
			healthData.Value = destructable.CurrentHealth / destructable.MaxHealth;
		}
	}

	/// <summary>
	/// Called when the player is destroyed (health reaches zero).
	/// Triggers death animation and raises player death event.
	/// </summary>
	/// <param name="damageInfo">Information about the fatal damage</param>
	public void OnDestroyed(DamageInfo damageInfo)
	{
		if (animator != null) animator.SetTrigger("Death");
		onPlayerDeathEvent?.RaiseEvent(gameObject);
	}

	/// <summary>
	/// Handle collisions with other objects, applying push forces to rigidbodies.
	/// Allows player to push physics objects in the game world.
	/// </summary>
	/// <param name="hit">Information about the collision</param>
	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		var rb = hit.collider.attachedRigidbody;

		// Skip if there's no rigidbody, it's kinematic, or hit is from above
		if (rb == null || rb.isKinematic || hit.moveDirection.y < -0.3f) return;

		// Apply horizontal push force only
		Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
		rb.linearVelocity = pushDir * playerData.pushForce;
	}
}