using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using System.Collections.Generic;
using MonkeyBusiness.Combat.Health;
using System.Linq;
using UnityEngine.AI;

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


        [SerializeField]
        float _impactForce = 20f;

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
 

        [SerializeField]
        [Tooltip("Time for which the projectile sticks to the target after hitting, before being destroyed.")]
        float _stickTime = 1f;

        public LayerMask DestroyedBy => _destroyedBy;

        float _travelTime = 0f;

        bool _sticks = false;

        bool _isStuck = false;

        Vector3 _stickPosition;

        MeshRenderer _renderer;

        SortedSet<ProjectileHitInfo> _targetsByTime;

        TrailRenderer _trailRenderer;


        void Awake()
        {
            _trailRenderer = GetComponentInChildren<TrailRenderer>();
            _renderer = GetComponentInChildren<MeshRenderer>();
            _renderer.enabled = false;
        }

        public void Initialize(Vector3 firePointDirection, float deathTime, SortedSet<ProjectileHitInfo> targetsByTime, bool sticks, Vector3 stickPosition)  
        {
            _sticks = sticks;
            Direction = firePointDirection.normalized;
            
            _targetsByTime = targetsByTime != null ? targetsByTime : new SortedSet<ProjectileHitInfo>();
            _deathTime = deathTime;

            _stickPosition = stickPosition;

            if(_targetsByTime == null)
                Debug.LogError("Projectile initialized with null targetsByTime set!");
        }


        void Update()
        {
             _travelTime += Time.deltaTime;
            /*if(!_renderer.enabled && _travelTime >= 0.1f)
            {
                _renderer.enabled = true;
            }*/

            if(!_isStuck)
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
                    if(!_sticks)
                    {
                        Destroy(gameObject); // If the projectile didn't hit anything stickable, it gets destroyed after reaching its max lifetime.
                    }
                    else
                    {
                        Debug.Log("Sticking!");
                        _trailRenderer.enabled = false;
                        _isStuck = true;
                        transform.forward = Direction;
                        transform.position = _stickPosition;
                        _travelTime = 0f;
                    }
                }
            }
            else // If the projectile hit something and is now sticking to it, it will be destroyed after the stick time elapses.
            {
                if(_travelTime >= _stickTime)
                {
                    Destroy(gameObject);
                }
            }
        }

        void CheckForHit()
        {
            float hitTime = _targetsByTime.First().HitTime;

            while(_targetsByTime.Count > 0 && hitTime <= _travelTime)
            {    
                GameObject target = _targetsByTime.First().Target;
                _targetsByTime.Remove(_targetsByTime.First());
                if(target != null)
                {
                    var targetHealth = target.GetComponentInParent<HealthController>();
                    if(targetHealth == null)
                    {
                        Debug.LogError("Target does not have a HealthController component!");
                        continue;
                    }
                    _onTargetHit.Invoke(target);
                    if(_impactForce > 0f)
                    {
                        ApplyImpact(target);
                        targetHealth.TakeDamage(Damage * DamageMultiplier, Direction * _impactForce * Time.deltaTime);
                    }
                    else targetHealth.TakeDamage(Damage * DamageMultiplier, Direction);
                }

            }
        }

        void ApplyImpact(GameObject target)
        {
            var targetAgent = target.GetComponentInParent<NavMeshAgent>();

            var calculatedImpact = Direction * _impactForce * Time.deltaTime;;
            Debug.Log("Calculated impact: " + calculatedImpact);
            if(targetAgent == null)
            {
                Debug.LogError("Target does not have a NavMeshAgent component!");
            }
            else if(targetAgent.enabled)
            {
                Debug.Log("Applying impact to NavMeshAgent: " + calculatedImpact);
                targetAgent.velocity += calculatedImpact;
            }
            else
            {
                var targetRb = target.GetComponentInParent<Rigidbody>();
                if(targetRb != null)
                {
                    Debug.Log("Applying impact to Rigidbody: " + calculatedImpact);
                    targetRb.AddForce(calculatedImpact, ForceMode.Impulse);
                }
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, HitboxRadius);
        }
    }
}
