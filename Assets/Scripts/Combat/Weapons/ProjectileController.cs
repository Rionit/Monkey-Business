using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using MonkeyBusiness.Combat.Health;

namespace MonkeyBusiness.Combat.Weapons
{
    /// <summary>
    /// Controls the behavior of projectiles.
    /// </summary>
    public class ProjectileController : MonoBehaviour, IProjectile
    {
        /// <summary>
        /// Damage dealt by the projectile on hit.
        /// </summary>
        [field: SerializeField]
        [field: BoxGroup("Stats")]
        [Tooltip("Damage dealt by the projectile on hit.")]
        public float Damage { get; set; } = 10f;

        /// <summary>
        /// Damage multiplier.
        /// </summary>
        [field: SerializeField]
        [field: BoxGroup("Stats")]
        [Tooltip("Multiplies the damage by this amount.")]
        public float DamageMultiplier { get; set; } = 1f;
        
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
        /// <remarks> <i> Directly works with the collider's radius <color=red>! RELATIVE TO SCALE !</color> </i> </remarks> 
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
        UnityEvent<GameObject> _onTargetHit = new();

        public UnityEvent<GameObject> OnTargetHit => _onTargetHit;

        [SerializeField]
        [Tooltip("Mask of layers that can destroy the projectile on contact (e.g. walls, obstacles).")]
        LayerMask _destroyedBy;

        public LayerMask DestroyedBy => _destroyedBy;
        
        List<GameObject> _objectsHit = new List<GameObject>(); // List to keep track of objects already hit by the projectile (for piercing projectiles in the future)

        public void Initialize(string EnemyTag, Vector3 firePointDirection)
        {
            TargetTag = EnemyTag;
            Direction = firePointDirection.normalized;
        }

        void FixedUpdate()
        {
            float frameDistance = Speed * Time.fixedDeltaTime;
            transform.Translate(frameDistance * Direction, Space.World);
            _travelledDistance += frameDistance;

            if(_travelledDistance >= MaxFlyDistance)
            {
                Debug.Log("Reached max distance");
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

            Debug.Log("Collided with " + other.gameObject.name);    
            if(_objectsHit.Contains(other.gameObject)) return; // Skip if this collider was already hit (for piercing projectiles in the future)
            _objectsHit.Add(other.gameObject);
            
            if(other.CompareTag(TargetTag)) 
            {
                _onTargetHit?.Invoke(other.gameObject);
                var targetHealth = other.GetComponentInParent<HealthController>();
                if(targetHealth == null)
                {
                    Debug.LogError("Target does not have a HealthController component!");
                    return;
                }
                targetHealth.TakeDamage(Damage * DamageMultiplier);
            }

            if((_destroyedBy.value & (1 << other.gameObject.layer)) != 0)
            {
                //Debug.Log("Destroying projectile");
                Destroy(gameObject); // Destroys the projectile if it hits an object that destroys it.
            }
        }
    }
}
