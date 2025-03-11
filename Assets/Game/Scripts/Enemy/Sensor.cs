using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour
{
	[SerializeField] private Transform origin;
	[SerializeField] private string targetTag;
	[SerializeField] private float distance;
	[SerializeField] private float senseRate;

	// game object that has been sensed
	public GameObject Sensed { get; private set; } = null;

	void Start()
	{
		StartCoroutine(SenseCoroutine());
	}

	IEnumerator SenseCoroutine()
	{
		while (true)
		{
			Sense();
			yield return new WaitForSeconds(senseRate);

		}
	}

	void Sense()
	{
		Sensed = null;

		Ray ray = new Ray(origin.position, origin.forward);
		Debug.DrawRay(ray.origin, ray.direction * distance, Color.red, 1.0f);
		if (Physics.Raycast(ray, out RaycastHit raycastHit, distance))
		{
			if (raycastHit.collider.CompareTag(targetTag)) 
			{
				print("sensed");
				Sensed = raycastHit.collider.gameObject;
			}
		}
	}
}
