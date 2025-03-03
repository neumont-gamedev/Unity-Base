using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ammo : MonoBehaviour, IInteractable
{
	[SerializeField] protected AmmoData ammoData;

	public void OnInteractStart(InteractorInfo info)
	{
		// apply damage if game object is damagable
		if (!ammoData.damageOverTime && info.gameObject.TryGetComponent<IDamageable>(out IDamageable damagable))
		{
			damagable.ApplyDamage(info.CreateDamageInfo(ammoData.damage, DamageInfo.DamageType.Ammo));
		}

		// create impact prefab
		if (ammoData.impactPrefab != null)
		{
			Instantiate(ammoData.impactPrefab, transform.position, transform.rotation);
		}

		// destroy game object
		if (ammoData.destroyOnImpact)
		{
			Destroy(gameObject);
		}
	}

	public void OnInteractActive(InteractorInfo info)
	{
		// apply damage if game object is damagable
		if (ammoData.damageOverTime && gameObject.TryGetComponent<IDamageable>(out IDamageable damagable))
		{
			damagable.ApplyDamage(info.CreateDamageInfo(ammoData.damage * Time.deltaTime, DamageInfo.DamageType.Ammo));
		}
	}

	public void OnInteractEnd(InteractorInfo info)
	{
		//
	}
}
