using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace MonkeyBusiness.Enemies
{
    public class EnemyMoveController : MonoBehaviour
    {
        [Required]
        [SerializeField]
        [Tooltip("NavMesh agent used for navigating")]
        NavMeshAgent _navMeshAgent;

        [SerializeField]
        [Tooltip("Target to chase")]
        GameObject _chaseTarget;

        [SerializeField]
        [ReadOnly]
        [Tooltip("Default speed obtained from the NavMeshAgent at Awake.")]
        [BoxGroup("Debug/Speed")]
        float _defaultMaxSpeed;

        [ShowInInspector]
        [ReadOnly]
        [Tooltip("Current speed of the NavMeshAgent.")]
        [BoxGroup("Debug/Speed")]
        public float CurrentMaxSpeed
        {
            get => _navMeshAgent.speed;
            private set
            {
                _navMeshAgent.speed = value;
            }
        }

        [SerializeField]
        [ReadOnly]
        [Tooltip("Default angular speed obtained from the NavMeshAgent at Awake.")]
        [BoxGroup("Debug/Speed")]
        float _defaultMaxAngularSpeed;

        [ShowInInspector]
        [ReadOnly]
        [Tooltip("Current angular speed of the NavMeshAgent.")]
        [BoxGroup("Debug/Speed")]
        public float CurrentAngularSpeed
        {
            get => _navMeshAgent.angularSpeed;
            private set
            {
                _navMeshAgent.angularSpeed = value;
            }
        }

        [SerializeField]
        [ReadOnly]
        [Tooltip("Default acceleration obtained from the NavMeshAgent at Awake.")]
        [BoxGroup("Debug/Acceleration")]
        float _defaultAcceleration;

        [ShowInInspector]
        [ReadOnly]
        [Tooltip("Current acceleration of the NavMeshAgent.")]
        [BoxGroup("Debug/Acceleration")]
        public float CurrentAcceleration
        {
            get => _navMeshAgent.acceleration;
            private set
            {
                _navMeshAgent.acceleration = value;
            }
        }

        [SerializeField]
        [ReadOnly]
        [Tooltip("Default stopping distance obtained from the NavMeshAgent at Awake.")]
        [BoxGroup("Debug")]
        float _defaultStoppingDistance;

        [ShowInInspector]
        [ReadOnly]
        [Tooltip("Current stopping distance of the NavMeshAgent.")]
        [BoxGroup("Debug")]
        public float CurrentStoppingDistance
        {
            get => _navMeshAgent.stoppingDistance;
            private set
            {
                _navMeshAgent.stoppingDistance = value;
            }
        }

        /// <summary>
        /// Avoidance priority range for the <see cref="_navMeshAgent"/>.
        /// </summary>
        /// <remarks> <i> The actual avoidance priority will be a random value within this range.</i></remarks>
        [SerializeField]
        [MinMaxSlider(0, 100)]
        [Tooltip("Avoidance priority range for the NavMeshAgent.<br/><br/> <i> The actual avoidance priority will be a random value within this range. </i>")]
        Vector2Int _avoidancePriorityRange = new Vector2Int(20, 50);
        
        [SerializeField]
        [Tooltip("Interval in seconds at which the enemy updates its path to the target.")]
        float _updatePathInterval = 1f;

        [SerializeField]
        [Range(0f, 30f)]
        [Tooltip("Distance the enemy will try to keep from the player")]
        float _chaseDistance = 0f;

        Vector3 _currentTargetPos;

        [ShowInInspector]
        [ReadOnly]
        [Tooltip("Whether the enemy is running <color=red>towards to</color> or <color=green>away from</color> the target.")]
        bool _runningAway = false;

        float _timeTillPathUpdate = 0f;

        void Awake()
        {
            _navMeshAgent.avoidancePriority = Random.Range(_avoidancePriorityRange.x, _avoidancePriorityRange.y);   
            _timeTillPathUpdate = _updatePathInterval;
            SetupDefaultValues();
        }

        void SetupDefaultValues()
        {
            _defaultMaxSpeed = _navMeshAgent.speed;
            _defaultMaxAngularSpeed = _navMeshAgent.angularSpeed;
            _defaultAcceleration = _navMeshAgent.acceleration;
            _defaultStoppingDistance = _navMeshAgent.stoppingDistance;
        }

        /// <summary>
        /// Sets the NavMeshAgent's values to their defaults obtained at Awake. 
        /// </summary>
        public void SetDefaultValues()
        {
            CurrentMaxSpeed = _defaultMaxSpeed;
            CurrentAngularSpeed = _defaultMaxAngularSpeed;
            CurrentAcceleration = _defaultAcceleration;
            CurrentStoppingDistance = _defaultStoppingDistance;
        }

        void UpdatePosition()
        {
            if(_chaseTarget == null) return;
            var distance = Vector3.Distance(transform.position, _chaseTarget.transform.position);
            if(_runningAway && distance >= _chaseDistance * 1.1f)
            {
                _runningAway = false;
            }
            else if(!_runningAway && distance <= _chaseDistance * 0.9f)
            {
                _runningAway = true;
            }

            _currentTargetPos = _runningAway ? GetRunawayPosition() : _chaseTarget.transform.position;
            _navMeshAgent.SetDestination(_currentTargetPos);
        }

        /// <summary>
        /// Gets the position the enemy should run to in order to maintain the desired distance from the target. Only used when <see cref="_runningAway"/> is true.
        /// </summary>
        Vector3 GetRunawayPosition()
        {
            Vector3 directionToTarget = (_chaseTarget.transform.position - transform.position).normalized;
            return _chaseTarget.transform.position - directionToTarget * _chaseDistance * 1.1f;
        }

        void FixedUpdate()
        {
            _timeTillPathUpdate -= Time.fixedDeltaTime;
            if(_timeTillPathUpdate <= 0f)
            {
                UpdatePosition();
                _timeTillPathUpdate = _updatePathInterval;
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = _runningAway ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, _chaseDistance);
            if(_currentTargetPos != null )
            {
                Gizmos.DrawLine(transform.position, _currentTargetPos);
            }
        }
    }
}
