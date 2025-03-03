using UnityEngine;

/// <summary>
/// Contains detailed information about an interaction event.
/// Used to provide context about how an interaction occurred and who initiated it.
/// </summary>
public struct InteractorInfo
{
	/// <summary>
	/// Categorizes the type of entity performing the interaction.
	/// </summary>
	public enum InteractorType { Player, Enemy, NPC, Object }

	/// <summary>
	/// Reference to the GameObject that performed the interaction.
	/// </summary>
	public GameObject gameObject;

	/// <summary>
	/// Classification of what kind of entity the interactor is.
	/// </summary>
	public InteractorType type;

	/// <summary>
	/// World position where the interaction occurred.
	/// For collisions, this is the contact point.
	/// </summary>
	public Vector3 hitPoint;

	/// <summary>
	/// Direction from which the interaction came.
	/// For collisions, this is typically opposite to the surface normal.
	/// </summary>
	public Vector3 hitDirection;

	/// <summary>
	/// Surface normal at the point of interaction.
	/// Useful for reflection calculations or orienting effects.
	/// </summary>
	public Vector3 hitNormal;

	/// <summary>
	/// Relative velocity at the moment of interaction.
	/// Only applicable for collision-based interactions.
	/// </summary>
	public Vector3 collisionVelocity;

	/// <summary>
	/// Creates a damage info structure based on this interaction information.
	/// </summary>
	/// <param name="damageAmount">Amount of damage to apply</param>
	/// <param name="damageType">Type of damage being inflicted</param>
	/// <returns>A DamageInfo structure with relevant collision data</returns>
	public DamageInfo CreateDamageInfo(float damageAmount, DamageInfo.DamageType damageType)
	{
		return new DamageInfo
		{
			inflictor = this,
			amount = damageAmount,
			hitPoint = this.hitPoint,
			hitDirection = this.hitDirection,
			type = damageType
		};
	}
}


/// <summary>
/// Structure containing detailed information about damage events.
/// Used to pass damage data between components in a standardized format.
/// </summary>
public struct DamageInfo
{
	/// <summary>
	/// Categorizes the type of damage being inflicted.
	/// </summary>
	public enum DamageType { Physical, Fire, Fall, Zone }

	/// <summary>
	/// Information about the entity that inflicted the damage.
	/// </summary>
	public InteractorInfo inflictor;

	/// <summary>
	/// The amount of damage to apply to the target.
	/// </summary>
	public float amount;

	/// <summary>
	/// World position where the damage occurred.
	/// Used for effects and directional calculations.
	/// </summary>
	public Vector3 hitPoint;

	/// <summary>
	/// Direction from which the damage was inflicted.
	/// Can be used for calculating knockback or damage reactions.
	/// </summary>
	public Vector3 hitDirection;

	/// <summary>
	/// The type of damage being applied.
	/// Used for damage resistances and special effects.
	/// </summary>
	public DamageType type;
}

/// <summary>
/// Base class for objects that can interact with IInteractable components.
/// Provides filtering and validation for interaction events.
/// </summary>
public class Interactor : MonoBehaviour
{
	/// <summary>
	/// Optional tag filter. If specified, only objects with this tag will trigger interactions.
	/// If left empty, interactions will occur with any object implementing IInteractable.
	/// </summary>
	[SerializeField]
	[Tooltip("Optional tag filter. If specified, only objects with this tag will trigger interactions. Leave empty to interact with any IInteractable object.")]
	protected string tagName;

	/// <summary>
	/// Defines which layers this interactor will interact with.
	/// Only objects on the specified layers will be considered for interaction.
	/// Leave as "Everything" to interact with objects on all layers.
	/// </summary>
	[SerializeField]
	[Tooltip("Defines which layers this interactor will interact with. Only objects on the specified layers will be considered for interaction.")]
	protected LayerMask interactionLayers = Physics.AllLayers;

	/// <summary>
	/// Determines if the given GameObject passes the tag and layer filters for interaction.
	/// This method checks if the object meets the basic requirements before attempting 
	/// to interact with it.
	/// </summary>
	/// <param name="other">The GameObject to check against filters</param>
	/// <returns>True if the object passes all filtering requirements, false otherwise</returns>
	virtual protected bool IsValidInteraction(GameObject other)
	{
		// Check if no tag filter is set or if the other object has the specified tag
		if (string.IsNullOrEmpty(tagName) || other.CompareTag(tagName))
		{
			// Check if the other object is on a valid interaction layer
			if ((interactionLayers & (1 << other.layer)) != 0)
			{
				return true;
			}
		}
		return false;
	}
}