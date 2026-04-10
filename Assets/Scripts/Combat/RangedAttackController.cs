using Sirenix.OdinInspector;
using UnityEditor.Build.Content;
using UnityEngine;
using MonkeyBusiness.Managers;
using System.Runtime.CompilerServices;
using System.Collections;

[assembly: InternalsVisibleTo("MonkeyBusiness.Tests")]
namespace MonkeyBusiness.Combat
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

        [SerializeField]
        [Tooltip("Assign the attack invoker <color=green>manually</color> (<color=green>true</color>) " +
        "or <color=yellow>automatically</color> from current game object (<color=yellow>false</color>)")]
        bool _manuallyAssignInvoker = false;

        [SerializeField]
        [ShowIf(nameof(_manuallyAssignInvoker))]
        AttackInvoker _attackInvoker;

        [SerializeField]
        [Tooltip("Prefab of the projectile to be fired.")]
        GameObject _projectilePrefab;

        [SerializeField]
        [Tooltip("Where to spawn projectiles")]
        Transform _firePoint;

        public void ExecuteAttack(GameObject target)
        {
            lastPlayerPos = target.transform.position;
            StartCoroutine(RangedAttackCoroutine(target));
        }

        Vector3 lastPlayerPos;

        IEnumerator RangedAttackCoroutine(GameObject target)
        {
            yield return new WaitForSeconds(ChargeTime); // Waits for the charge
            var projectile = Instantiate(_projectilePrefab, _firePoint.position, Quaternion.identity, GameManager.Instance.ProjectileParent.transform);
            var projectileController = projectile.GetComponent<ProjectileController>();
            if (projectileController != null)
            {
                Debug.Log("Has projectile controller");
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

        void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(_firePoint.position, lastPlayerPos - _firePoint.position);
        }
    }
}