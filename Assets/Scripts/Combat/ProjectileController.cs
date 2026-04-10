using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonkeyBusiness.Combat
{
    /// <summary>
    /// Controls the behavior of projectiles.
    /// </summary>
    public class ProjectileController : MonoBehaviour
    {
        /// <summary>
        /// Damage dealt by the projectile on hit.
        /// </summary>
        [field:SerializeField]
        [Tooltip("Damage dealt by the projectile on hit.")]
        public float Damage { get; private set; } = 10f;

        /// <summary>
        /// Speed of the projectile. [Units/s]
        /// </summary>
        [field: SerializeField]
        [Tooltip("Speed of the projectile. [Units/s]")]
        public float Speed { get; private set; } = 10f;

        /// <summary>
        /// Radious of the projectile hitbox.
        /// </summary>
        /// <remarks> <i> Directly works with the collider's radius. </i> </remarks> 
        public float HitboxRadius
        {
            get
            {
                return _collider != null ? _collider.radius : float.NaN;
            }
            set
            {
                if(_collider != null) _collider.radius = value;
            }
        }

        [ShowInInspector]
        [Tooltip("Assign the target collider <color=green>manually</color> (<color=green>true</color>) " +
        "or <color=yellow>automatically</color> from current game object (<color=yellow>false</color>)")]
        bool _manuallyAssignCollider = false;

        [ShowIf(nameof(_manuallyAssignCollider))]
        [Tooltip("Collider of the projectile")]
        SphereCollider _collider;

        [ShowInInspector]
        [ReadOnly]
        [BoxGroup("Debug")]
        string _targetTag;

        [ShowInInspector]
        [ReadOnly]
        [BoxGroup("Debug")]
        Vector3 _direction;

        /// <summary>
        /// Initializes the projectile with the given parameters.
        /// </summary>
        public void Initialize(string enemyTag, Vector3 direction)
        {
            _targetTag = enemyTag;
            this._direction = direction;
            transform.forward = direction.normalized;
        }

        void FixedUpdate()
        {
            transform.Translate(transform.forward * Speed * Time.fixedDeltaTime);
        }

        void Awake()
        {
            if(!_manuallyAssignCollider)
                _collider = GetComponent<SphereCollider>();
            if(_collider == null)
            {
                Debug.LogError("Projectile requires a SphereCollider component!");
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag(_targetTag)) 
            {
                var targetHealth = other.GetComponentInParent<HealthController>();
                if(targetHealth == null)
                {
                    Debug.LogError("Target does not have a HealthController component!");
                    return;
                }
                targetHealth.TakeDamage(Damage);
            }
            Destroy(gameObject); // Destroys the projectile
        }
    }
}
