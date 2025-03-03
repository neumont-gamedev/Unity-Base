using UnityEngine;

/// <summary>
/// Applies damage to objects implementing IDamageable when interaction occurs.
/// Works with the interaction system to handle hazards, traps, and damaging objects.
/// Can perform one-time or continuous damage and spawn visual effects.
/// </summary>
public class DamageInteractable : MonoBehaviour, IInteractable
{
	[Header("Damage Settings")]
	[SerializeField]
	[Tooltip("Type of damage this source inflicts (Physical, Fire, Fall, Zone, etc.)")]
	private DamageInfo.DamageType damageType = DamageInfo.DamageType.Physical;

	[SerializeField]
	[Tooltip("Amount of damage to deal per hit. Higher values cause more damage.")]
	private float damage = 1;

	[SerializeField]
	[Tooltip("Whether to destroy this GameObject after dealing damage. Set to true for one-time damage sources.")]
	private bool destroyOnHit = true;

	[SerializeField]
	[Tooltip("Layers that can receive damage from this source. Only objects on these layers will be damaged.")]
	private LayerMask damageableLayers = Physics.AllLayers;

	[SerializeField]
	[Tooltip("Visual effect to spawn when damage is dealt. Leave empty for no effect.")]
	private GameObject hitFxPrefab = null;

	[SerializeField]
	[Tooltip("Minimum time between damage applications for continuous damage. Lower values allow more frequent damage.")]
	private float damageRate = 0.1f;

	/// <summary>
	/// Tracks when damage was last dealt for rate limiting continuous damage.
	/// </summary>
	private float lastDamageTime;

	#region IInteractable Implementation

	/// <summary>
	/// Determines if interaction with this damage source is allowed.
	/// By default, allows all interactions to proceed to damage calculation.
	/// </summary>
	/// <param name="info">Information about the interacting entity</param>
	/// <returns>True to allow the interaction, false to prevent it</returns>
	public bool CanInteract(InteractorInfo info) => true;

	/// <summary>
	/// Called when an object first interacts with this damage source.
	/// Attempts to apply damage immediately on initial contact.
	/// </summary>
	/// <param name="info">Information about the interacting entity including collision data</param>
	public void OnInteractStart(InteractorInfo info)
	{
		// Process the interaction as damage
		TryApplyDamage(info);
	}

	/// <summary>
	/// Called continuously while an object is interacting with this damage source.
	/// Applies damage at the rate specified by damageRate, useful for damage-over-time effects.
	/// </summary>
	/// <param name="info">Information about the interacting entity including collision data</param>
	public void OnInteractActive(InteractorInfo info)
	{
		// Apply continuous damage with rate limiting
		if (Time.time < lastDamageTime + damageRate) return;
		TryApplyDamage(info);
	}

	/// <summary>
	/// Called when an object stops interacting with this damage source.
	/// No damage is applied during this phase by default.
	/// </summary>
	/// <param name="info">Information about the interacting entity</param>
	public void OnInteractEnd(InteractorInfo info)
	{
		// No special behavior needed when interaction ends
	}

	#endregion

	/// <summary>
	/// Attempts to apply damage to an interacting object if it meets all requirements.
	/// Checks layer compatibility and searches for IDamageable components.
	/// </summary>
	/// <param name="info">Information about the interacting entity</param>
	private void TryApplyDamage(InteractorInfo info)
	{
		// Early exit if target is not on a damageable layer
		if (!OnDamageLayer(info.gameObject)) return;

		// Try to get the damageable component from the hit object
		if (info.gameObject.TryGetComponent(out IDamageable damageable))
		{
			// Create damage info with collision and damage details
			var damageInfo = new DamageInfo
			{
				inflictor = info,
				amount = damage,
				hitPoint = info.hitPoint,
				hitDirection = info.hitDirection,
				type = damageType
			};

			// Apply damage to the target
			damageable.ApplyDamage(damageInfo);

			// Spawn hit effect if one is set
			if (hitFxPrefab != null)
			{
				Instantiate(hitFxPrefab, damageInfo.hitPoint, Quaternion.LookRotation(damageInfo.hitDirection));
			}

			// Destroy this object if configured to do so
			if (destroyOnHit)
			{
				Destroy(gameObject);
			}

			// Update timestamp for rate limiting
			lastDamageTime = Time.time;
		}
	}

	/// <summary>
	/// Determines if the target GameObject is on a layer configured to receive damage.
	/// Uses bitwise operations to check if the target's layer is in the damageableLayers mask.
	/// </summary>
	/// <param name="target">The GameObject to check against the layer mask</param>
	/// <returns>True if the target is on a damageable layer, false otherwise</returns>
	private bool OnDamageLayer(GameObject target)
	{
		return (damageableLayers.value & (1 << target.layer)) != 0;
	}
}