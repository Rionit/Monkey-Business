using UnityEngine;
using MonkeyBusiness.Combat.Health;
using MonkeyBusiness.Combat.Attack;
using Sirenix.OdinInspector;
using MonkeyBusiness.Enemies.Navigation;
using System;
using MonkeyBusiness.Managers;
using System.Collections.Generic;
using MonkeyBusiness.Enemies.Visuals;


namespace MonkeyBusiness.Enemies.Behavior
{
    using Random = UnityEngine.Random;

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
        [Tooltip("Poop attack collider used for the shitfest perk effect")]
        Collider _poopCollider;
        
        [BoxGroup("Components")]
        [SerializeField]
        [Required]
        [Tooltip("Renderer used to change the gorilla's material when enraged")]
        Renderer _renderer;

        [BoxGroup("Components")]
        [SerializeField]
        EnemyMaterialController _materialController;

        [FoldoutGroup("Rage")]
        [SerializeField]
        [PreviewField(100, ObjectFieldAlignment.Right)]
        Material _enragedMaterial;

        [FoldoutGroup("Rage")]
        [SerializeField]
        [Tooltip("Health percentage threshold for the rage effect to trigger")]
        [Range(0f, 100f)]
        float _rageHealthThreshold = 50f;

        [FoldoutGroup("Rage")]
        [SerializeField]
        [Tooltip("Multiplier for the enemy's speed when enraged")]
        [Range(1f, 3f)]
        float _speedMultiplier = 1.5f;

        [FoldoutGroup("Rage")]
        [SerializeField]
        [Tooltip("Multiplier for the enemy's damage when enraged")]
        [Range(1f, 5f)]
        float _damageMultiplier = 1.5f;

        [FoldoutGroup("Rage")]
        [SerializeField]
        [Tooltip("Multiplier for the enemy's charge time when enraged")]
        [Range(0.1f, 1f)]
        float _chargeTimeMultiplier = 0.75f; // Faster charge time for more challenge
        
        [FoldoutGroup("Rage")]
        [ShowInInspector]
        [ReadOnly]
        [Tooltip("Whether the gorilla has already enraged (to prevent multiple enrages)")]
        bool _hasRaged = false;

        [ShowInInspector]
        [FoldoutGroup("Rage")]
        [ReadOnly]
        public float AbsoluteHealthThreshold => _health.MaxHealth * (_rageHealthThreshold / 100f);


        [SerializeField]
        [Tooltip("Index of the material to change when enraged (if the renderer has multiple materials)")]
        [FoldoutGroup("Rage")]
        int _enragedMaterialIndex = 0;

        void Awake()
        {
            _enragedMaterialIndex = _materialController.Order == EnemyMaterialController.MaterialOrder.SKIN_CLOTHING ? 0 : 1;
            _health.OnHealthChanged.AddListener(EnrageIfLow);
            StatsManager.Instance.onNonChimpCanPoop.AddListener(OnCanPoop);
            OnCanPoop(StatsManager.Instance.canNonChimpPoop);
        }

        void OnCanPoop(bool status)
        {
            _poopCollider.enabled = status;
        }

        private void OnDestroy()
        {
            _health.OnHealthChanged.RemoveListener(EnrageIfLow);
            SetEnragedMaterital(false);
            StatsManager.Instance.onNonChimpCanPoop.RemoveListener(OnCanPoop);
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


                SetEnragedMaterital(true);
            }
        }

        void SetEnragedMaterital(bool enraged)
        {
            var materials = new Material[_renderer.materials.Length];
            Array.Copy(_renderer.materials, materials, materials.Length);

            materials[_enragedMaterialIndex] = enraged ? _enragedMaterial : _materialController.UsedSkinMaterial;
            _renderer.materials = materials;

            Debug.Log("Changing material to " + (enraged ? _enragedMaterial.name : _materialController.UsedSkinMaterial.name));
        }
    }
}