using Sirenix.OdinInspector;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Collections;
using MonkeyBusiness.Combat.Weapons;

[assembly: InternalsVisibleTo("MonkeyBusiness.Tests")]
namespace MonkeyBusiness.Combat.Attack
{
    /// <summary>
    /// Controls the behavior of ranged attacks.
    /// </summary>
    public class RangedAttackController : MonoBehaviour, IAttack
    {
        [field: BoxGroup("Stats")]
        [field:SerializeField]
        [Tooltip("Damage of the attack, in health units.")]
        public float Damage {get; private set;} = 10f;

        [field: BoxGroup("Stats")]
        [field:SerializeField]
        [Tooltip("Time before the attack actually fires, in seconds.")]
        public float ChargeTime {get; private set;} = 0.5f;

        internal void TestSetup(GameObject projectilePrefab, Transform firePoint)
        {
            _projectilePrefab = projectilePrefab;
            _firePoint = firePoint;
        }

        [BoxGroup("Invoker")]
        [SerializeField]
        [Tooltip("Assign the attack invoker <color=green>manually</color> (<color=green>true</color>) " +
        "or <color=yellow>automatically</color> from current game object (<color=yellow>false</color>)")]
        bool _manuallyAssignInvoker = false;

        [BoxGroup("Invoker")]
        [SerializeField]
        [ShowIf(nameof(_manuallyAssignInvoker))]
        AttackInvoker _attackInvoker;

        [BoxGroup("Projectile")]
        [Required]
        [SerializeField]
        [Tooltip("Prefab of the projectile to be fired.")]
        GameObject _projectilePrefab;

        [BoxGroup("Projectile")]
        [SerializeField]
        [Required]
        [Tooltip("Where to spawn projectiles")]
        Transform _firePoint;

        public void ExecuteAttack(GameObject target)
        {
            StartCoroutine(RangedAttackCoroutine(target));
        }

        IEnumerator RangedAttackCoroutine(GameObject target)
        {
            yield return new WaitForSeconds(ChargeTime); // Waits for the charge
            var projectile = Instantiate(_projectilePrefab, _firePoint.position, Quaternion.identity, ProjectileParentHolder.Instance.Object.transform);
            var projectileController = projectile.GetComponent<ProjectileController>();
            if (projectileController != null)
            {
                projectileController.Initialize("Player", (target.transform.position - _firePoint.position));
            }
        }

        void Awake()
        {
            if(!_manuallyAssignInvoker || _attackInvoker == null)
                _attackInvoker = GetComponent<AttackInvoker>();
            if(_attackInvoker == null)
            {
                Debug.LogError("No AttackInvoker attached to attacker");
                return;
            }

            _attackInvoker.OnAttackInvoked.AddListener((target) => ExecuteAttack(target));
        }
    }
}