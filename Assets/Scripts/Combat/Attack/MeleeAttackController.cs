using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using MonkeyBusiness.Combat.Health;
using DG.Tweening;
using Ami.BroAudio;

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
        public float Damage { get; set; } = 20f;

        [field: BoxGroup("Stats")]
        [field: SerializeField]
        public float ChargeTime { get; set; } = 0.5f;

        [BoxGroup("Debug")]
        [ShowInInspector]
        [ReadOnly]
        [Tooltip("If the attack is currently charging")]
        bool _aboutToAttack = false;

        [SerializeField]
        Renderer _chargeRenderer;

        [SerializeField]
        GameObject _attackVFX;

        [SerializeField]
        Color _chargeFinalColor = new Color(188f/255f, 0f, 0f, 0.25f);

        [SerializeField]
        GameObject _animToggle;

        [SerializeField]
        SoundSource _chargeSFX;

        [SerializeField]
        SoundSource _attackSFX;

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

            _chargeSFX.Play();
            var tween = _chargeRenderer.material.DOColor(_chargeFinalColor, ChargeTime).From(new Color(188f/255f, 0f, 0f, 0f)).OnComplete(() => _chargeRenderer.material.color = new Color(188f/255f, 0f, 0f, 0f)); // TODO: Make the color and material properties to tween parameters
            Debug.Log("Started animating");
            yield return new WaitForSeconds(ChargeTime); // Waits for the charge

            _animToggle.SetActive(true);
            _attackVFX.SetActive(true);
            _attackSFX.Play();
            
            Debug.Log("activated attack vfx");
            _aboutToAttack = false; // Makes the attack
            // TODO: Attack animation will be here

            // If the player gets hit by the attack, it takes damage
            if(_attackInvoker.PlayerInRange)
            {
                targetHealth.TakeDamage(Damage, Vector3.zero);
            }

            yield return new WaitUntil(() => _animToggle.activeSelf == false);
            _attackVFX.SetActive(false);
            Debug.Log("Deactivated attack vfx");
        }

        void Awake()
        {
            _chargeRenderer.material.color = new Color(188f/255f, 0f, 0f, 0f);
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
