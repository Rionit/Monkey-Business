using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using System.Collections.Generic;
using MonkeyBusiness.Combat.Health;
using System.Linq;

namespace MonkeyBusiness.Combat.Weapons
{
    public class PlayerProjectileController : MonoBehaviour, IProjectile
    {
        public class ProjectileHitInfo
        {
            public GameObject Target;
            public float HitTime;

            public ProjectileHitInfo(GameObject target, float hitTime)
            {
                Target = target;
                HitTime = hitTime;
            }
        }

        public class ProjectileHitInfoComparer : IComparer<ProjectileHitInfo>
        {
            public int Compare(ProjectileHitInfo x, ProjectileHitInfo y)
            {
                return x.HitTime.CompareTo(y.HitTime);
            }
        }

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

        [field:SerializeField]
        [field: BoxGroup("Stats")]
        [Tooltip("Maximum distance the projectile can fly before being destroyed.")]
        public float MaxFlyDistance { get; private set; } = 100f;

        [field: Tooltip("Radius of the projectile hitbox.\n\n<color=red>!ABSOLUTE!</color>")]
        [field:SerializeField]
        public float HitboxRadius { get; private set; } = 0.5f;

        float _deathTime;

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

        float _travelTime = 0f;

        SortedSet<ProjectileHitInfo> _targetsByTime;

        public void Initialize(Vector3 firePointDirection, float deathTime, SortedSet<ProjectileHitInfo> targetsByTime)  
        {
            Direction = firePointDirection.normalized;
            _targetsByTime = targetsByTime;
            _deathTime = deathTime;

            if(_targetsByTime == null)
                Debug.LogError("Projectile initialized with null targetsByTime set!");
        }

        void Update()
        {
            float frameDistance = Speed * Time.deltaTime;
            transform.position += Direction * frameDistance;
            _travelTime += Time.deltaTime;

            if(_targetsByTime.Count > 0)
            {
                CheckForHit();
            }
            if(_travelTime >= _deathTime)
            {
                Destroy(gameObject);
            }
        }

        void CheckForHit()
        {
            float hitTime = _targetsByTime.First().HitTime;

            while(_targetsByTime.Count > 0 && hitTime <= _travelTime)
            {    
                GameObject target = _targetsByTime.First().Target;

                if(target != null)
                {

                    _onTargetHit.Invoke(target);

                    var targetHealth = target.GetComponentInParent<HealthController>();
                    if(targetHealth == null)
                    {
                        Debug.LogError("Target does not have a HealthController component!");
                        continue;
                    }
                    targetHealth.TakeDamage(Damage * DamageMultiplier);
                }
                _targetsByTime.Remove(_targetsByTime.First());
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, HitboxRadius);
        }
    }
}
