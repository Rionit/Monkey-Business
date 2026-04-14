using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using MonkeyBusiness.Combat.Health;

namespace MonkeyBusiness.Combat.Attack
{
    [RequireComponent(typeof(AttackInvoker))]
    public class MeleeAttackController : MonoBehaviour, IAttack
    {
        [BoxGroup("Invoker")]
        [SerializeField]
        [ShowInInspector]
        [Tooltip("Assign the attack invoker <color=green> manually </color> (<color=green>true</color>) " 
        + " or <color=yellow>automatically</color> from current game object (<color=yellow>false</color>)")]
        bool _manuallyAssignInvoker = false;


        [BoxGroup("Invoker")]
        [ShowIf(nameof(_manuallyAssignInvoker))]
        [SerializeField]
        AttackInvoker _attackInvoker;
        
        [field: BoxGroup("Stats")]
        [field: SerializeField]
        public float Damage { get; private set; } = 20f;

        [field: BoxGroup("Stats")]
        [field: SerializeField]
        public float ChargeTime { get; private set; } = 0.5f;

        [BoxGroup("Debug")]
        [ShowInInspector]
        [ReadOnly]
        [Tooltip("If the attack is currently charging")]
        bool _aboutToAttack = false;

        public void ExecuteAttack(GameObject target)
        {
            StartCoroutine(MeleeAttackCoroutine(target));
        }

        IEnumerator MeleeAttackCoroutine(GameObject target)
        {
            // Gets the player's health
            var targetHealth = target.GetComponentInParent<HealthController>();
            if(targetHealth == null)
            {
                Debug.LogError("Target does not have a HealthController component in itself or parents!");
                yield break;
            }
            _aboutToAttack = true;
            yield return new WaitForSeconds(ChargeTime); // Waits for the charge

            _aboutToAttack = false; // Makes the attack
            // TODO: Attack animation will be here

            // If the player gets hit by the attack, it takes damage
            if(_attackInvoker.PlayerInRange)
            {
                targetHealth.TakeDamage(Damage);
            }
        }

        void Awake()
        {
            if(!_manuallyAssignInvoker)
                _attackInvoker = GetComponent<AttackInvoker>();
            if(_attackInvoker == null)
            {
                Debug.LogError("No AttackInvoker attached to attacker");
                return;
            }

            _attackInvoker.OnAttackInvoked.AddListener(ExecuteAttack);            
        }
        
        void OnDrawGizmos()
        {
            if(_aboutToAttack)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(transform.position, _attackInvoker.AttackRange);
            }
        }
    }
}
