using UnityEngine;
using MonkeyBusiness.Combat.Health;
using MonkeyBusiness.Combat.Attack;
using Sirenix.OdinInspector;


namespace MonkeyBusiness.Enemies
{
    /// <summary>
    /// Controls special behavior of the gorilla enemy
    /// </summary>
    public class GorillaController : MonoBehaviour
    {
        [BoxGroup("Components")]
        [SerializeField]
        [Required]
        [Tooltip("Health controller used for the rage effect")]
        HealthController _health;

        [BoxGroup("Components")]
        [SerializeField]
        [Required]
        [Tooltip("Enemy follow controller used for the rage effect")]
        EnemyFollowController _followController;

        [BoxGroup("Components")]
        [SerializeField]
        [Required]
        [Tooltip("Melee attack controller used for the rage effect")]
        MeleeAttackController _attackController;

        [BoxGroup("Components")]
        [SerializeField]
        [Required]
        [Tooltip("Renderer used to change the gorilla's material when enraged")]
        Renderer _renderer;

        [BoxGroup("Rage/Materials")]
        [SerializeField]
        [PreviewField(100, ObjectFieldAlignment.Right)]
        Material _enragedMaterial;

        [BoxGroup("Rage")]
        [SerializeField]
        [Tooltip("Health percentage threshold for the rage effect to trigger")]
        [Range(0f, 100f)]
        float _rageHealthThreshold = 50f;

        [BoxGroup("Rage")]
        [SerializeField]
        [Tooltip("Multiplier for the enemy's speed when enraged")]
        [Range(1f, 3f)]
        float _speedMultiplier = 1.5f;

        [BoxGroup("Rage")]
        [SerializeField]
        [Tooltip("Multiplier for the enemy's damage when enraged")]
        [Range(1f, 5f)]
        float _damageMultiplier = 1.5f;

        [BoxGroup("Rage")]
        [SerializeField]
        [Tooltip("Multiplier for the enemy's charge time when enraged")]
        [Range(0.1f, 1f)]
        float _chargeTimeMultiplier = 0.75f; // Faster charge time for more challenge
        
        [BoxGroup("Rage")]
        [ShowInInspector]
        [ReadOnly]
        [Tooltip("Whether the gorilla has already enraged (to prevent multiple enrages)")]
        bool _hasRaged = false;

        [ShowInInspector]
        [ReadOnly]
        public float AbsoluteHealthThreshold => _health.MaxHealth * (_rageHealthThreshold / 100f);

        void Awake()
        {
            _health.OnHealthChanged.AddListener(EnrageIfLow);
        }

        /// <summary>
        /// Applies the rage if the gorilla's health drops below the threshold.
        /// </summary>
        void EnrageIfLow(float currentHealth)
        {
            if (!_hasRaged && currentHealth < AbsoluteHealthThreshold)
            {
                _hasRaged = true;
                _followController.ChangeDefaultValues(_speedMultiplier);
                _attackController.Damage *= _speedMultiplier; // Increase damage as well for more challenge
                _attackController.ChargeTime *= _chargeTimeMultiplier; // Decrease charge time for more challenge
                _renderer.material = _enragedMaterial;
            }
        }
    }
}