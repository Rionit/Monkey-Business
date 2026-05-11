using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine.Events;
using MonkeyBusiness.Misc;

namespace MonkeyBusiness.Combat.Attack
{
    // TODO: Better range checking for ranged enemies (raycasting, pre-fire)
    /// <summary>
    /// Controls the attack range and cooldown. Invokes attack when 
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class AttackInvoker : MonoBehaviour
    {
        /// <summary>
        /// Collider representing the attack range.
        /// </summary>
        [BoxGroup("Collider")]
        [SerializeField]
        [Required]
        [Tooltip("Collider representing the attack range")]
        SphereCollider _attackRangeCollider;

        [SerializeField]
        [HideInInspector]
        float _attackRange = 3f; // Fallback range if collider is not assigned yet

        [BoxGroup("Stats")]
        [ShowInInspector]
        [Range(0.1f, 30f)]
        [Tooltip("Radius of the attack range")]
        public float AttackRange
        {
            get
            {
                return _attackRangeCollider != null ? _attackRangeCollider.radius : _attackRange;
            }
            set
            {
                if(_attackRangeCollider != null)
                    _attackRangeCollider.radius = value;
                _attackRange = value;
            } 
        }

        [SerializeField]
        [HideInInspector]
        float _cooldownTime = 5f;

        /// <summary>
        [BoxGroup("Stats")]
        [ShowInInspector]
        [Tooltip("Cooldown time between attacks, in seconds")]
        public float CooldownTime 
        {
            get => _cooldownTime;
            set 
            {
                _cooldownTime = value;
                _attackSpeed = 1f / _cooldownTime;
            }
        }

        [SerializeField]
        [HideInInspector]
        float _attackSpeed = 0.2f;


        [ShowInInspector]
        [Tooltip("Attack speed of the entity, in attacks per second. \n\n <color=green><i> = 1/CooldownTime</i></color>")]
        public float AttackSpeed 
        {
            get => _attackSpeed;
            set
            {
                _attackSpeed = value;
                _cooldownTime = 1f / _attackSpeed;   
            }
        }

        const float RAYCAST_CHECK_INTERVAL = 0.5f;

        /// <summary>
        /// Whether the player is in the attack range or not.
        /// </summary>
        [ShowInInspector]
        [Tooltip("Whether the player is in the attack range or not")]
        [ReadOnly]
        [BoxGroup("Debug")]
        public bool PlayerInRange {get; private set;} = false;      

        /// <summary>
        /// Whether the attack is currently on cooldown or not.
        /// </summary>
        [ShowInInspector]
        [Tooltip("Whether the attack is currently on cooldown or not")]
        [ReadOnly]
        [BoxGroup("Debug")]
        public bool OnCooldown { get; private set;} = false;

        #if UNITY_EDITOR
        [BoxGroup("Debug")]
        [ShowInInspector]
        [SerializeField]
        [Tooltip("Color of the attack range gizmo in the editor")]
        Color _debugGizmoColor = new Color(0.5f, 0.5f, 0.8f, 0.35f);
        #endif

        /// <summary>
        /// Event invoked when the attack is triggered. The attacked target's object is passed as a parameter.
        /// </summary>
        public UnityEvent<GameObject> OnAttackInvoked = new();

        float _timeWhenDisabled = float.MinValue;

        GameObject _checkedTarget;

        void OnTriggerEnter(Collider other)
        {
            if(other.tag == "Player")
            {
                PlayerInRange = true;
                _checkedTarget = other.gameObject;
                if(!OnCooldown)
                {
                    StartCoroutine(InvokeAttackCoroutine(other.GetComponent<ITargetable>().Target));
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if(other.tag == "Player")
            {
                PlayerInRange = false;
            }
        }

        void Awake()
        {
            if(_attackRangeCollider == null)
            {
                _attackRangeCollider = GetComponent<SphereCollider>(); 
            }
            _attackRangeCollider.radius = AttackRange;
        }

        void OnEnable()
        {
            var currentTime = Time.time;
            if(OnCooldown && currentTime - _timeWhenDisabled < CooldownTime)
            {
                StartCoroutine(WaitAndStartChecking(CooldownTime - (currentTime - _timeWhenDisabled)));
            }
             else if(PlayerInRange && _checkedTarget != null)
            {
                StartCoroutine(CheckAttackCoroutine(_checkedTarget));   
            }
        }

                void OnDisable()
        {
            _timeWhenDisabled = Time.time; 
        }

        IEnumerator WaitAndStartChecking(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            if(_checkedTarget != null && PlayerInRange)
            {
                StartCoroutine(CheckAttackCoroutine(_checkedTarget));
            }
        }



        IEnumerator CheckAttackCoroutine(GameObject target)
        {
            while(PlayerInRange)
            {
                bool inSight = !Physics.Raycast(transform.position, (target.transform.position - transform.position).normalized, out RaycastHit hit, Vector3.Distance(target.transform.position, transform.position), LayerMask.GetMask("Default", "Navigation", "Swing"));
                if(inSight) // The enemy can directly see the target -> attack
                {
                    if(!OnCooldown)
                    {
                        Debug.Log("Player in sight - invoking attack");
                        StartCoroutine(InvokeAttackCoroutine(target));
                        yield break; 
                    }
                    else yield return new WaitForSeconds(RAYCAST_CHECK_INTERVAL); // Wait for cooldown to expire and check again
                }
                else // The enemy cannot directly see the target -> check again after a short delay
                {
                    Debug.Log("Enemy not in sight - waiting");
                    yield return new WaitForSeconds(RAYCAST_CHECK_INTERVAL);
                }        
            }
        }

        /// <summary>
        /// Invokes the attack and waits for the cooldown to expire.
        /// </summary>
        /// <param name="target">The attack target.</param>
        /// <remarks> Might start another attack coroutine after cooldown expiration. </remarks>
        IEnumerator InvokeAttackCoroutine(GameObject target)
        {
            Debug.Log("Attack about to be invoked - waiting for cooldown");
            OnCooldown = true;
            OnAttackInvoked.Invoke(target);
            yield return new WaitForSeconds(CooldownTime);
            OnCooldown = false;
            if(PlayerInRange)
            {
                Debug.Log("Cooldown expired and player still in range - checking attack again");
                StartCoroutine(CheckAttackCoroutine(target));
            }
        }

        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = _debugGizmoColor;
            Gizmos.DrawSphere(transform.position, AttackRange);
        }
        #endif
    }
}
