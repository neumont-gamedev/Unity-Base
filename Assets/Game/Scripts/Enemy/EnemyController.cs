using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.Collections;
using Unity.VisualScripting;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
	[SerializeField] private Transform[] waypoints;
	[SerializeField] private Sensor sensor;
	[SerializeField] private Animator animator;

	private NavMeshAgent navMeshAgent;
	private Transform waypoint;
	private GameObject target;

	enum State
	{
		Idle,
		Patrol,
		Chase,
		Attack,
		Death,
		Wait
	}

	private State state = State.Idle;
	private float senseTimer = 0;

	void Start()
	{
		navMeshAgent = GetComponent<NavMeshAgent>();
		if (waypoints == null || waypoints.Length == 0)
		{
			waypoints = GameObject.FindGameObjectsWithTag("Waypoint").Select(go => go.transform).ToArray();
		}

		StartCoroutine(OnIdle());
	}

	void Update()
	{
		target = sensor.Sensed;

		switch (state)
		{
			case State.Idle:
				//				
				break;
			case State.Patrol:
				// reached the waypoint, set a new destination
				if (navMeshAgent.remainingDistance < 0.5f)
				{
					waypoint = waypoints[Random.Range(0, waypoints.Length)];
					navMeshAgent.SetDestination(waypoint.position);
				}
				// check for player
				if (target != null)
				{
					state = State.Chase;
				}
				break;
			case State.Chase:
				navMeshAgent.isStopped = false;
				if (target != null)
				{
					navMeshAgent.SetDestination(target.transform.position);
					senseTimer = 2;

					float distance = Vector3.Distance(transform.position, target.transform.position);
					if (distance < 2)
					{
						StopAllCoroutines();
						StartCoroutine(OnAttack());
					}
				}

				senseTimer -= Time.deltaTime;
				if (senseTimer <= 0)
				{
					StopAllCoroutines();
					StartCoroutine(OnPatrol());
				}
				break;
			case State.Attack:
				break;
			case State.Death:
				break;
			case State.Wait:
				//
			break;
			default:
				break;
		}
	}

	IEnumerator OnIdle()
	{
		state = State.Idle;
		navMeshAgent.isStopped = true;

		yield return new WaitForSeconds(1.0f);

		StartCoroutine(OnPatrol());
	}

	IEnumerator OnPatrol()
	{
		state = State.Patrol;
		navMeshAgent.isStopped = false;
		
		waypoint = waypoints[Random.Range(0, waypoints.Length)];
		navMeshAgent.SetDestination(waypoint.position);

		yield return new WaitForSeconds(Random.Range(4, 8));

		StartCoroutine(OnIdle());
	}

	IEnumerator OnAttack()
	{
		state = State.Attack;
		animator?.SetTrigger("Attack");
		yield return new WaitForSeconds(0.5f);
		OnAnimAttack();

		StartCoroutine(OnPatrol());
	}

	void OnAnimAttack()
	{
		var colliders = Physics.OverlapSphere(transform.position, 2);
		foreach (var collider in colliders)
		{
			if (collider.gameObject.CompareTag("Player"))
			{
				if (collider.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
				{
					print("damage");
					//damageable.ApplyDamage(10);
					break;
				}
			}
		}
	}
}

