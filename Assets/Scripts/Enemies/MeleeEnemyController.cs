using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class MeleeEnemyController : MonoBehaviour, IEnemyController
{

    /// <summary>
    /// Agent used for pathfinding.
    /// </summary>
    [SerializeField]
    [Tooltip("Agent used for pathfinding")]
    NavMeshAgent navMeshAgent;

    [SerializeField]
    float damage = 10;

    // TODO: Change to private
    public Transform _playerTransform;

    /// <summary>
    /// Cooldown before recalculating path to player, in seconds.
    /// </summary>
    float _recalcCD = 0.3f;

    float _currentTime = 0f;

    float _originalAccel;

    void Start()
    {
        _originalAccel = navMeshAgent.acceleration;
    }

    public void Initialize(Transform playerTransform)
    {
        _playerTransform = playerTransform;
    }

    void FixedUpdate()
    {
        _currentTime += Time.fixedDeltaTime;
        if(_currentTime >= _recalcCD)
        {
            navMeshAgent.SetDestination(_playerTransform.position);
            _currentTime = 0f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Melee collision with " + other.gameObject.name);
        if(other.gameObject.tag == "Player")
        {
            var playerHealth = other.GetComponentInParent<HealthController>();
            if(playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }

    public IEnumerator Slowdown(float duration, float slowdownFactor)
    {
        navMeshAgent.acceleration = _originalAccel * slowdownFactor;
        yield return new WaitForSeconds(duration);
        navMeshAgent.acceleration = _originalAccel;
    }
}