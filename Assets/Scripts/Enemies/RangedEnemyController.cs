using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class RangedEnemyController : MonoBehaviour, IEnemyController
{

    /// <summary>
    /// Agent used for pathfinding.
    /// </summary>
    [SerializeField]
    [Tooltip("Agent used for pathfinding")]
    NavMeshAgent navMeshAgent;


    [SerializeField]
    GameObject projectile;

    // TODO: Change to private
    public Transform _playerTransform;

    /// <summary>
    /// Cooldown before recalculating path to player, in seconds.
    /// </summary>
    float _recalcCD = 1f;

    float _currentTime = 0f;

    [SerializeField]
    float runawayDistance = 10f;

    [SerializeField]
    float attackRange = 10f;


    [SerializeField]
    Transform bulletSpawnPoint;

    [SerializeField]
    public GameObject projectileParent;

    [SerializeField]
    float attackCooldown = 1f;

    bool _attackOnCD = false;

    public void Initialize(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }

    void RunAwayFromPlayer(float currentDistance)
    {
        Debug.Log("Running away");

        Vector3 vectorFromPlayer = transform.position - _playerTransform.position;

        float distanceToPlayer = vectorFromPlayer.magnitude;
        Vector3 runAwayDirection = vectorFromPlayer.normalized * (runawayDistance - distanceToPlayer + 1f);

        runAwayDirection.z = 0;
        Vector3 runAwayPerp = new Vector3(-runAwayDirection.z, 0, runAwayDirection.x);

        Vector3 pos = Vector3.zero;
        if(NavMesh.SamplePosition(transform.position + runAwayDirection, out var hit, 5f, NavMesh.AllAreas))
        {
            pos = hit.position;
        }
        else if(NavMesh.SamplePosition(transform.position + runAwayPerp, out var hit2, 5f, NavMesh.AllAreas))
        {
            pos = hit2.position;
        }
        else if (NavMesh.SamplePosition(transform.position - runAwayPerp, out var hit3, 5f, NavMesh.AllAreas))
        {
            pos = hit3.position;
        }
        else Debug.LogError("Couldn't find a valid position to run away to!");
        Debug.Log("Runaway target: " + pos + " ... player target: " + _playerTransform.position);
        navMeshAgent.SetDestination(pos);
    }


    IEnumerator AttackPlayer()
    {
        Debug.Log("Attacking player");
        var proj = Instantiate(projectile, bulletSpawnPoint.position, Quaternion.LookRotation(_playerTransform.position - bulletSpawnPoint.position));
        
        proj.transform.parent = projectileParent.transform;
        proj.GetComponent<ProjectileController>().Initialize("Player");

        yield return new WaitForSeconds(attackCooldown);
        _attackOnCD = false;
    }

    public IEnumerator Slowdown(float duration, float slowdownFactor)
    {
        var originalAccel = navMeshAgent.acceleration;
        navMeshAgent.acceleration *= slowdownFactor;
        yield return new WaitForSeconds(duration);
        navMeshAgent.acceleration = originalAccel;
    }

    void FixedUpdate()
    {
        _currentTime += Time.fixedDeltaTime;

        if(_currentTime >= _recalcCD)
        {
            var distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);
            
            if(distanceToPlayer < runawayDistance)
            {
                RunAwayFromPlayer(distanceToPlayer);
            }
            else if(distanceToPlayer > attackRange)
            {
                // Chase player
                Debug.Log("Chasing player");
                navMeshAgent.SetDestination(_playerTransform.position);
            }
            if (distanceToPlayer < attackRange && !_attackOnCD)
            {
                StartCoroutine(AttackPlayer());
            }
            // Setting to -delta time because it's gonna be incremented to 0
            _currentTime = 0;
        }
    }
}
