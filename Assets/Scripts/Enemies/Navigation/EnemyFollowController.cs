using System.Collections;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.PlayerLoop;

[assembly: InternalsVisibleTo("MonkeyBusiness.Tests")]
namespace MonkeyBusiness.Enemies.Navigation
{
    public class EnemyFollowController : MonoBehaviour
    {
        const float ZONE_STOPPING_DISTANCE = 7f;

        [Required]
        [SerializeField]
        [Tooltip("NavMesh agent used for navigating")]
        NavMeshAgent _navMeshAgent;

        [field: SerializeField]
        [field: Tooltip("Target to chase")]
        public GameObject ChaseObject { get; set; } 

        [SerializeField]
        [ReadOnly]
        [Tooltip("Default speed obtained from the NavMeshAgent at Awake.")]
        [BoxGroup("Debug/Speed")]
        float _defaultMaxSpeed;

        [ShowInInspector]
        [Tooltip("Current speed of the NavMeshAgent.")]
        [BoxGroup("Stats")]
        public float CurrentMaxSpeed
        {
            // Null check is only to avoid error in the Editor (at first, no NavMeshAgent is assigned). 
            get => _navMeshAgent == null ? float.NaN : _navMeshAgent.speed;
            set
            {
                if (_navMeshAgent == null) return;
                _navMeshAgent.speed = value;
            }
        }

        [SerializeField]
        [ReadOnly]
        [Tooltip("Default angular speed obtained from the NavMeshAgent at Awake.")]
        [BoxGroup("Debug/Speed")]
        float _defaultMaxAngularSpeed;

        [ShowInInspector]
        [Tooltip("Current angular speed of the NavMeshAgent.")]
        [BoxGroup("Stats")]
        public float CurrentAngularSpeed
        {
            get => _navMeshAgent == null ? float.NaN : _navMeshAgent.angularSpeed;
            set
            {
                if (_navMeshAgent == null) return;
                _navMeshAgent.angularSpeed = value;
            }
        }

        [SerializeField]
        [ReadOnly]
        [Tooltip("Default acceleration obtained from the NavMeshAgent at Awake.")]
        [BoxGroup("Debug/Acceleration")]
        float _defaultAcceleration;

        [ShowInInspector]
        [Tooltip("Current acceleration of the NavMeshAgent.")]
        [BoxGroup("Stats")]
        public float CurrentAcceleration
        {
            get => _navMeshAgent == null ? float.NaN : _navMeshAgent.acceleration;
            set
            {
                if (_navMeshAgent == null) return;
                _navMeshAgent.acceleration = value;
            }
        }

        [SerializeField]
        [ReadOnly]
        [Tooltip("Default stopping distance obtained from the NavMeshAgent at Awake.")]
        [BoxGroup("Debug")]
        float _defaultStoppingDistance;

        [ShowInInspector]
        [Tooltip("Current stopping distance of the NavMeshAgent.")]
        [BoxGroup("Stats")]
        public float CurrentStoppingDistance
        {
            get => _navMeshAgent == null ? float.NaN : _navMeshAgent.stoppingDistance;
            set
            {
                if (_navMeshAgent == null) return;
                _navMeshAgent.stoppingDistance = value;
            }
        }

        /// <summary>
        /// Avoidance priority range for the <see cref="_navMeshAgent"/>.
        /// </summary>
        /// <remarks> <i> The actual avoidance priority will be a random value within this range.</i></remarks>
        [SerializeField]
        [MinMaxSlider(0, 100)]
        [Tooltip("Avoidance priority range for the NavMeshAgent.\n\n<i>The actual avoidance priority will be a random value within this range. </i>")]
        Vector2Int _avoidancePriorityRange = new Vector2Int(20, 50);


        [SerializeField]
        [Tooltip("Minimum movement multiplier for a slowdown effect, relative to default values.")]
        private float _minimumMovementMultiplier = 0.4f;

        [SerializeField]
        [Tooltip("Interval in seconds at which the enemy updates its path to the target.")]
        float _updatePathInterval = 1f;

        [SerializeField]
        [Range(0f, 30f)]
        [Tooltip("Distance the enemy will try to keep from the player")]
        float _chaseDistance = 0f;

        [SerializeField]
        [Range(0f, 2f)]
        [Tooltip("Offset from the chase distance in which the enemy will switch between running towards and away from the target." +
        "\n\n<i>Prevents jittery movement when close to the chase distance.</i>")]
        float _chaseDistanceOffset = 0.1f;

        [SerializeField]
        [Range(0f, 50f)]
        [Tooltip("Distance from the player at which the enemy will always chase them directly.")]
        float _alwaysChaseDistance = 10f;

        Vector3 _currentTargetPos;

        [ShowInInspector]
        [ReadOnly]
        [Tooltip("Whether the enemy is running <color=red>towards to</color> or <color=green>away from</color> the target.")]
        bool _runningAway = false;


        [BoxGroup("Slowdown Effect")]
        [SerializeField]
        [Tooltip("Whether the enemy has a slowdown effect or not.")]
        bool _hasSlowdownEffect = false;

        [BoxGroup("Slowdown Effect")]
        [SerializeField]
        [ShowIf(nameof(_hasSlowdownEffect))]
        [Tooltip("All visualizers of the slowdown effect.")]
        List<Renderer> _slowdownVisualizers = new List<Renderer>();

        [ShowInInspector]
        public List<TrafficZone> Path { get; internal set; } = new();

        [ShowInInspector]
        public TrafficZone CurrentZone { get; internal set; } = null;

        public bool ChasingPlayer => Path.Count <= 1;

        int _currentlyActiveEffects = 0;

        float _timeTillPathUpdate = 0f;

        Coroutine _slowdownCoroutine;

        void Awake()
        {
            _navMeshAgent.avoidancePriority = Random.Range(_avoidancePriorityRange.x, _avoidancePriorityRange.y);
            _timeTillPathUpdate = _updatePathInterval;
            SetupDefaultValues();
        }

        void Start()
        {
            TrafficManager.Instance.OnSoftRepath.AddListener(GetAlteringPath);
            TrafficManager.Instance.OnHardRepath.AddListener(GetNewPath);


            CurrentZone = TrafficManager.Instance.GetZone(transform.position);
            GetNewPath();
        }

        void OnDestroy()
        {
            if(TrafficManager.Instance != null)
            {
                TrafficManager.Instance.OnSoftRepath.RemoveListener(GetAlteringPath);
                TrafficManager.Instance.OnHardRepath.RemoveListener(GetNewPath);
                TrafficManager.Instance.ClearPath(this);
            }
        }

        void SetupDefaultValues()
        {
            _defaultMaxSpeed = _navMeshAgent.speed;
            _defaultMaxAngularSpeed = _navMeshAgent.angularSpeed;
            _defaultAcceleration = _navMeshAgent.acceleration;
            _defaultStoppingDistance = _navMeshAgent.stoppingDistance;
        }

        /// <summary>
        /// Slows the enemy's movement for a certain duration by multiplying its speed, angular speed and acceleration by the given multiplier. Values will not go below the defaults multiplied by <see cref="_minimumMovementMultiplier"/>.
        /// </summary>
        public void Slowdown(float duration, float speedMultiplier)
        {
            if (_slowdownCoroutine != null)
            {
                StopCoroutine(_slowdownCoroutine);
            }
            _slowdownCoroutine = StartCoroutine(SlowdownCoroutine(duration, speedMultiplier));
        }

        void AddSlowdownVisualizer()
        {
            if(_currentlyActiveEffects >= _slowdownVisualizers.Count)
            {
                Debug.LogWarning("Not enough slowdown effect renderers assigned! Please assign at least " + (_currentlyActiveEffects + 1) + " renderers.");
            }
            else 
            {
                _slowdownVisualizers[_currentlyActiveEffects++].enabled = true;
            }
        }

        void ResetSlowdownVisualizers()
        {
            foreach(var visualizer in _slowdownVisualizers)
            {
                visualizer.enabled = false;
            }

            _currentlyActiveEffects = 0;
        }

        IEnumerator SlowdownCoroutine(float duration, float speedMultiplier)
        {
            var prevMaxSpeed = CurrentMaxSpeed;
            CurrentMaxSpeed = Mathf.Max(CurrentMaxSpeed * speedMultiplier, _defaultMaxSpeed * _minimumMovementMultiplier);

            if(CurrentMaxSpeed < prevMaxSpeed && _hasSlowdownEffect)
            {
                AddSlowdownVisualizer();
            }

            CurrentAngularSpeed = Mathf.Max(CurrentAngularSpeed * speedMultiplier, _defaultMaxAngularSpeed * _minimumMovementMultiplier);
            CurrentAcceleration = Mathf.Max(CurrentAcceleration * speedMultiplier, _defaultAcceleration * _minimumMovementMultiplier);
            Debug.Log($"Applying slowdown effect with multiplier {speedMultiplier} for duration {duration} seconds.");

            yield return new WaitForSeconds(duration);

            Debug.Log("Slowdown effect ended, resetting values.");

            ResetSlowdownVisualizers();
            SetDefaultValues();
        }

        /// <summary>
        /// Changes the default values by a multiplier, removing all debuffs.
        /// </summary>
        public void ChangeDefaultValues(float multiplier)
        {
            // Removes slowdown debuff if there is any
            if(_slowdownCoroutine != null)
            {
                StopCoroutine(_slowdownCoroutine);
            }

            // Resets default values
            _defaultMaxSpeed = _defaultMaxSpeed * multiplier;
            _defaultMaxAngularSpeed = _defaultMaxAngularSpeed * multiplier;
            _defaultAcceleration = _defaultAcceleration * multiplier;
            //_defaultStoppingDistance = _defaultStoppingDistance * multiplier;
            
            // Applies new default values
            SetDefaultValues();
        }

        internal void TestSetup(float chaseDistance)
        {
            _chaseDistance = chaseDistance;
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

        void UpdatePlayerPosition()
        {
            CurrentStoppingDistance = _defaultStoppingDistance;
            if (ChaseObject == null) return;
            // TODO: Change
            //var flankingPos = _flankingController.GetMovementTarget(this);

            var distance = Vector3.Distance(transform.position, ChaseObject.transform.position);
            if (_runningAway && distance >= _chaseDistance * 1.1f)
            {
                _runningAway = false;
            }
            else if (!_runningAway && distance <= _chaseDistance * 0.9f)
            {
                _runningAway = true;
            }

            _currentTargetPos = _runningAway ? GetRunawayPosition() : ChaseObject.transform.position;
            _navMeshAgent.SetDestination(_currentTargetPos);
        }

        void GetAlteringPath()
        {
            if(enabled)
            {
                var playerZone = TrafficManager.Instance.PlayerZone;
                if(!TrafficManager.Instance.TryAlterPath(this, playerZone))
                {
                    TrafficManager.Instance.GetPath(this, playerZone);        
                }

                if(Path.Count > 1)
                {
                    ChaseObject = Path[0].Keypoint.gameObject;
                    MoveToKeypoint();
                }
                else
                {
                    ChaseObject = TrafficManager.Instance.Player;
                    UpdatePlayerPosition();
                }
            }
        }

        void GetNewPath()
        {
            if(enabled)
            {
                var playerZone = TrafficManager.Instance.PlayerZone;
                TrafficManager.Instance.GetPath(this, playerZone);        

                if(Path.Count > 1)
                {
                    ChaseObject = Path[0].Keypoint.gameObject;
                    MoveToKeypoint();
                }
                else
                {
                    ChaseObject = TrafficManager.Instance.Player;
                    UpdatePlayerPosition();
                }
            }
        }

        /// <summary>
        /// Gets the position the enemy should run to in order to maintain the desired distance from the target. Only used when <see cref="_runningAway"/> is true.
        /// </summary>
        Vector3 GetRunawayPosition()
        {
            Vector3 directionToTarget = (ChaseObject.transform.position - transform.position).normalized;
            return ChaseObject.transform.position - directionToTarget * _chaseDistance;
        }

        void OnEnable()
        {
            UpdatePlayerPosition();
        }

        void OnDisable()
        {
            if(TrafficManager.Instance != null)
            {
                TrafficManager.Instance.ClearPath(this);
            }
        }

        void FixedUpdate()
        {
            if(ChasingPlayer)
            {
                _timeTillPathUpdate -= Time.fixedDeltaTime;
                if (_timeTillPathUpdate <= 0f && _navMeshAgent.enabled)
                {
                    UpdatePlayerPosition();
                    _timeTillPathUpdate = _updatePathInterval;
                }
            }
            else
            {
                if(Vector3.Distance(transform.position, TrafficManager.Instance.Player.transform.position) <= _alwaysChaseDistance)
                {
                    ChaseObject = TrafficManager.Instance.Player;
                    TrafficManager.Instance.ClearPath(this);
                    UpdatePlayerPosition();
                }

                // If we reached a keypoint, move to some other
                if(Vector3.Distance(transform.position, ChaseObject.transform.position) <= _navMeshAgent.stoppingDistance)
                {
                    Path[0].RemoveEnemy(this);
                    Path.RemoveAt(0);
                    if(ChasingPlayer)
                    {
                        ChaseObject = TrafficManager.Instance.Player;
                        UpdatePlayerPosition();
                    }
                    else
                    {
                        ChaseObject = Path[0].Keypoint.gameObject;
                        MoveToKeypoint();
                    }
                }
            }
        }

        // Moves to next keypoint
        void MoveToKeypoint()
        {
            CurrentStoppingDistance = ZONE_STOPPING_DISTANCE;
            _navMeshAgent.SetDestination(ChaseObject.transform.position);            
        }

        void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            Gizmos.color = _runningAway ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, _chaseDistance);
            if (_currentTargetPos != null)
            {
                Gizmos.DrawLine(transform.position, _currentTargetPos);
            }

            if(Path.Count > 0)
            {
                Handles.color = Color.brown;
                if(Path.Count > 1)
                {
                    Handles.DrawLine(transform.position, Path[0].Keypoint.position, 2f);  
                    for(int i = 0; i < Path.Count - 2; i++)
                    {
                        Handles.DrawLine(Path[i].Keypoint.position, Path[i + 1].Keypoint.position, 2f);
                    }
                    Handles.DrawLine(Path[Path.Count - 2].Keypoint.position, TrafficManager.Instance.Player.transform.position, 2f);
                }
                else
                {
                    Handles.DrawLine(transform.position, TrafficManager.Instance.Player.transform.position, 2f);
                }
            }

            Handles.color = Color.yellow;

            if(ChaseObject != null)
                Handles.DrawLine(transform.position, ChaseObject.transform.position, 2f);
            #endif
        }
    }
}
