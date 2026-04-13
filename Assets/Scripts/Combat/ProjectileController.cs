using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

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
        [field: SerializeField]
        [field: BoxGroup("Stats")]
        [Tooltip("Damage dealt by the projectile on hit.")]
        public float Damage { get; set; } = 10f;

        /// <summary>
        /// Speed of the projectile. [Units/s]
        /// </summary>
        [field: SerializeField]
        [field: BoxGroup("Stats")]
        [Tooltip("Speed of the projectile. [Units/s]")]
        public float Speed { get; private set; } = 10f;

        float _hitboxRadius = 0.5f; // Fallback radius if collider is not assigned yet

        [ShowInInspector]
        [BoxGroup("Stats")]
        [Range(0.1f, 10f)]
        /// <summary>
        /// Radious of the projectile hitbox.
        /// </summary>
        /// <remarks> <i> Directly works with the collider's radius. </i> </remarks> 
        public float HitboxRadius
        {
            get
            {
                return _collider != null ? _collider.radius : _hitboxRadius;
            }
            private set
            {
                if(_collider != null) _collider.radius = value;
                _hitboxRadius = value;
            }
        }

        [field:SerializeField]
        [field: BoxGroup("Stats")]
        [Tooltip("Maximum distance the projectile can fly before being destroyed.")]
        public float MaxFlyDistance { get; private set; } = 100f;

        [BoxGroup("Collider")]
        [ShowInInspector]
        [Tooltip("Assign the target collider <color=green>manually</color> (<color=green>true</color>) " +
        "or <color=yellow>automatically</color> from current game object (<color=yellow>false</color>)")]
        bool _manuallyAssignCollider = false;

        [SerializeField]
        [BoxGroup("Collider")]
        [ShowIf(nameof(_manuallyAssignCollider))]
        [Tooltip("Collider of the projectile")]
        SphereCollider _collider;

        /// <summary>
        /// Tag of the target the projectile should hit.
        /// </summary>
        [ShowInInspector]
        [ReadOnly]
        [BoxGroup("Debug")]
        [Tooltip("Tag of the target the projectile should hit.")]
        public string TargetTag {get; private set; }

        /// <summary>
        /// Current direction the projectile is flying towards.
        /// </summary>
        /// <remarks><i>Normalized.</i></remarks>
        [ShowInInspector]
        [ReadOnly]
        [BoxGroup("Debug")]
        [Tooltip("Current direction the projectile is flying towards. <br/> <br/> <i>Normalized.</i>")]
        public Vector3 Direction {get; private set; }

        [ShowInInspector]
        [BoxGroup("Debug")]
        [ReadOnly]
        [Tooltip("Distance travelled by the projectile since it was fired.")]
        float _travelledDistance = 0f;

        /// <summary>
        /// Event invoked when the projectile hits its target, passing the hit GameObject as an argument.
        /// </summary>
        [SerializeField]
        [Tooltip("Event invoked when the projectile hits its target, passing the hit GameObject as an argument.")]
        public UnityEvent<GameObject> OnTargetHit = new();

        [SerializeField]
        [Tooltip("Mask of layers that can destroy the projectile on contact (e.g. walls, obstacles).")]
        LayerMask _destroyedBy;
        
        List<GameObject> _objectsHit = new List<GameObject>(); // List to keep track of objects already hit by the projectile (for piercing projectiles in the future)

        /// <summary>
        /// Initializes the projectile with the given parameters.
        /// </summary>
        public void Initialize(string enemyTag, Vector3 direction)
        {
            TargetTag = enemyTag;
            Direction = direction.normalized;

            // BUG: LookRotation + forward axis movement doesn't work for some reason (forward axis is not rotated properly)
            //transform.rotation = Quaternion.LookRotation(Direction, Vector3.up);
        }

        void FixedUpdate()
        {
            transform.Translate(Speed * Time.fixedDeltaTime * /*transform.forward*/ Direction);
            _travelledDistance += Speed * Time.fixedDeltaTime;
            if(_travelledDistance >= MaxFlyDistance)
            {
                Destroy(gameObject);
            }
        }

        void Awake()
        {
            if(!_manuallyAssignCollider)
            {
                _collider = GetComponent<SphereCollider>();
                _collider.radius = _hitboxRadius;
            }
            if(_collider == null)
            {
                Debug.LogError("Projectile requires a SphereCollider component!");
            }
            if(_destroyedBy == 0)
            {
                Debug.Log("Projectile destruction mask is empty, projectile will not be destroyed by any objects!");
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if(_objectsHit.Contains(other.gameObject)) return; // Skip if this collider was already hit (for piercing projectiles in the future)
            _objectsHit.Add(other.gameObject);
            
            if(other.CompareTag(TargetTag)) 
            {
                OnTargetHit?.Invoke(other.gameObject);
                var targetHealth = other.GetComponentInParent<HealthController>();
                if(targetHealth == null)
                {
                    Debug.LogError("Target does not have a HealthController component!");
                    return;
                }
                targetHealth.TakeDamage(Damage);
            }

            if((_destroyedBy.value & other.gameObject.layer) != 0)
            {
                Debug.Log("Destroying projectile");
                Destroy(gameObject); // Destroys the projectile if it hits an object that destroys it.
            }
        }
    }
}
