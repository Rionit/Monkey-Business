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
        [BoxGroup("Collider")]
        [SerializeField]
        [ShowInInspector]
        [Tooltip("Assign the collider <color=green>manually</color> (<color=green>true</color>) " 
        + "or <color=yellow>automatically</color> from current game object (<color=yellow>false</color>)")]
        bool _manuallyAssignCollider = false;

        /// <summary>
        /// Collider representing the attack range.
        /// </summary>
        [BoxGroup("Collider")]
        [SerializeField]
        [ShowIf(nameof(_manuallyAssignCollider))]
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

        float _cooldownTime = 5f;

        /// <summary>
        [BoxGroup("Stats")]
        [ShowInInspector]
        [Tooltip("Cooldown time between attacks, in seconds")]
        public float CooldownTime 
        {
            get => _cooldownTime;
            set => _cooldownTime = value;
        }

        [ShowInInspector]
        [Tooltip("Attack speed of the entity, in attacks per second. \n\n <color=green><i> = 1/CooldownTime</i></color>")]
        public float AttackSpeed {get; set; } = 0.2f;

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

        /// <summary>
        /// Event invoked when the attack is triggered. The attacked target's object is passed as a parameter.
        /// </summary>
        public UnityEvent<GameObject> OnAttackInvoked = new();

        void OnTriggerEnter(Collider other)
        {
            if(other.tag == "Player")
            {
                PlayerInRange = true;
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

        /// <summary>
        /// Invokes the attack and waits for the cooldown to expire.
        /// </summary>
        /// <param name="target">The attack target.</param>
        /// <remarks> Might start another attack coroutine after cooldown expiration. </remarks>
        IEnumerator InvokeAttackCoroutine(GameObject target)
        {
            OnCooldown = true;
            OnAttackInvoked.Invoke(target);
            yield return new WaitForSeconds(CooldownTime);
            OnCooldown = false;
            if(PlayerInRange)
            {
                StartCoroutine(InvokeAttackCoroutine(target));
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.5f, 0.5f, 0.8f, 0.35f);
            Gizmos.DrawSphere(transform.position, AttackRange);
        }
    }
}
