using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MeleeEnemyController : MonoBehaviour
{

    /// <summary>
    /// Agent used for pathfinding.
    /// </summary>
    [SerializeField]
    [Tooltip("Agent used for pathfinding")]
    NavMeshAgent navMeshAgent;


    // TODO: Change to private
    public Transform _playerTransform;

    /// <summary>
    /// Cooldown before recalculating path to player, in seconds.
    /// </summary>
    float _recalcCD = 1f;

    float _currentTime = 0f;

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
}
