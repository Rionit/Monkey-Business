using Sirenix.OdinInspector;
using UnityEditor.Build.Content;
using UnityEngine;
using MonkeyBusiness.Managers;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MonkeyBusiness.Tests")]
namespace MonkeyBusiness.Combat
{
    /// <summary>
    /// Controls the behavior of ranged attacks.
    /// </summary>
    public class RangedAttackController : MonoBehaviour, IAttack
    {
        [field:SerializeField]
        [Tooltip("Damage of the attack, in health units.")]
        public float Damage {get; private set;} = 10f;

        [field:SerializeField]
        [Tooltip("Time before the attack actually fires, in seconds.")]
        public float ChargeTime {get; private set;} = 0.5f;

        internal void TestSetup(GameObject projectilePrefab, Transform firePoint)
        {
            _projectilePrefab = projectilePrefab;
            _firePoint = firePoint;
        }

        [ShowInInspector]
        [Tooltip("Prefab of the projectile to be fired.")]
        GameObject _projectilePrefab;

        [ShowInInspector]
        [Tooltip("Where to spawn projectiles")]
        Transform _firePoint;

        public void ExecuteAttack(GameObject target)
        {
            var projectile = Instantiate(_projectilePrefab, _firePoint.position, Quaternion.identity, GameManager.Instance.ProjectileParent.transform);
            var projectileController = projectile.GetComponent<ProjectileController>();
            if (projectileController != null)
            {
                projectileController.Initialize("Player", (target.transform.position - _firePoint.position).normalized);
            }
        }
    }
}