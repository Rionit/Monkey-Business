using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine.Events;
using System.Runtime.CompilerServices;
using System.Reflection;
using UnityEngine.UIElements;

namespace MonkeyBusiness.Combat
{
    /// <summary>
    /// Controls the attack range and cooldown. Invokes attack when 
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class AttackInvoker : MonoBehaviour
    {
        [ShowInInspector]
        [Tooltip("Assign the collider manually (true) or automatically from current game object (false)")]
        bool _manuallyAssignCollider = false;

        /// <summary>
        /// Collider representing the attack range.
        /// </summary>
        [SerializeField]
        [ShowIf(nameof(_manuallyAssignCollider))]
        [Tooltip("Collider representing the attack range")]
        [Required]
        SphereCollider _attackRangeCollider;

        [ShowInInspector]
        [Tooltip("Radius of the attack range")]
        public float AttackRange
        {
            get
            {
                if(_attackRangeCollider != null) return _attackRangeCollider.radius;
                else return float.NaN;
            }
            set
            {
                if(_attackRangeCollider != null)
                    _attackRangeCollider.radius = value;
            } 
        }

        /// <summary>
        /// Cooldown time between attacks, in seconds.
        /// </summary>
        [field: SerializeField]
        [Tooltip("Cooldown time between attacks, in seconds")]
        public float CooldownTime {get; private set;} = 5f; 

        /// <summary>
        /// Whether the player is in the attack range or not.
        /// </summary>
        [ShowInInspector]
        [Tooltip("Whether the player is in the attack range or not")]
        public bool PlayerInRange {get; private set;} = false;      

        /// <summary>
        /// Whether the attack is currently on cooldown or not.
        /// </summary>
        [ShowInInspector]
        [Tooltip("Whether the attack is currently on cooldown or not")]
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
                    StartCoroutine(InvokeAttackCoroutine(other.gameObject));
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
