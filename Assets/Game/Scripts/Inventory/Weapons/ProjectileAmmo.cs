using UnityEngine;

/// <summary>
/// Handles physics-based projectile ammunition behavior.
/// Applies force to projectiles and manages their lifetime.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ProjectileAmmo : Ammo
{
	/// <summary>
	/// Reference to the rigidbody component for physics manipulation.
	/// </summary>
	private Rigidbody rb;

	/// <summary>
	/// Initializes the projectile, applying initial force and setting up destruction timer.
	/// </summary>
	private void Awake()
	{
		// Cache the rigidbody reference
		rb = GetComponent<Rigidbody>();
	}

	/// <summary>
	/// Applies initial force to the projectile and schedules its destruction.
	/// </summary>
	private void Start()
	{
		// Apply initial forward force based on ammo data
		if (ammoData.force != 0)
		{
			rb.AddRelativeForce(Vector3.forward * ammoData.force, ammoData.forceMode);
		}

		// Schedule object destruction after lifetime expires
		if (ammoData.hasLifetime) Destroy(gameObject, ammoData.lifetime);
	}
}