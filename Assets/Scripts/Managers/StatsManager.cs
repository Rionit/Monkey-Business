using System.Collections.Generic;
using MonkeyBusiness.Combat.Health;
using MonkeyBusiness.Player;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace MonkeyBusiness.Managers
{
    public class StatsManager : MonoBehaviour
    {
        public static StatsManager Instance { get; private set; }

        public UnityEvent<bool> onNonChimpCanPoop;

        public bool canNonChimpPoop = false;

        [ShowInInspector] public float PlayerMaxHealth
        {
            // Null check is only to avoid error in the Editor
            get => _healthController == null ? float.NaN : _healthController.MaxHealth;
            set
            {
                if (_healthController == null) return;
                _healthController.SetMaxHealth(value);
            }
        }
        
        [ShowInInspector] public float PlayerHealth
        {
            // Null check is only to avoid error in the Editor
            get => _healthController == null ? float.NaN : _healthController.CurrentHealth;
            set
            {
                if (_healthController == null) return;
                if (value < _healthController.CurrentHealth)
                    _healthController.TakeDamage(_healthController.CurrentHealth - value, Vector3.up);
                else if(value > _healthController.CurrentHealth)
                    _healthController.Heal(value - _healthController.CurrentHealth);
            }
        }

        [ShowInInspector] public float PlayerWalkSpeed
        {
            // Null check is only to avoid error in the Editor
            get => _characterController == null ? float.NaN : _characterController.WalkSpeed;
            set
            {
                if (_characterController == null) return;
                _characterController.WalkSpeed = value;
            }
        }

        private bool canUseRope = true;

        [ShowInInspector]
        public bool CanUseRope
        {
            get => _characterController != null
                ? _characterController.CanUseRope
                : canUseRope;

            set
            {
                canUseRope = value;

                if (_characterController != null)
                    _characterController.CanUseRope = value;
            }
        }
        
        [ShowInInspector]
        public float GetDamageMultiplier(GameObject prefab)
        {
            if (!_damageMultipliers.TryGetValue(prefab, out var multipliers) || multipliers.Count == 0)
                return 1f;

            float positiveBonus = 0f;
            float negativeMultiplier = 1f;

            foreach (var multiplier in multipliers)
            {
                if (multiplier > 1f)
                {
                    positiveBonus += multiplier;
                }
                else
                {
                    negativeMultiplier *= multiplier;
                }
            }

            return (1f + positiveBonus) * negativeMultiplier;
        }

        [ShowInInspector]
        public void AddDamageMultiplier(GameObject prefab, float amount)
        {
            if (!_damageMultipliers.TryGetValue(prefab, out var multipliers))
            {
                multipliers = new List<float>();
                _damageMultipliers[prefab] = multipliers;
            }

            multipliers.Add(amount);
        }
        
        [ShowInInspector]
        public bool RemoveDamageMultiplier(GameObject prefab, float amount)
        {
            if (!_damageMultipliers.TryGetValue(prefab, out var multipliers))
                return false;

            bool removed = multipliers.Remove(amount);

            if (multipliers.Count == 0)
                _damageMultipliers.Remove(prefab);

            return removed;
        }
        
        [ShowInInspector] private Dictionary<GameObject, List<float>> _damageMultipliers = new();
        
        [ShowInInspector] public Dictionary<ScriptableObject, bool> _perksUsage = new();
        
        private HealthController _healthController;
        public EquipmentManager _equipmentManager {get; private set;}
        private PlayerCharacter _characterController;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of StatsManager detected! Replacing the old one.");
            }

            Instance = this;

            GameManager gameManager = GetComponent<GameManager>();

            if (gameManager == null)
            {
                Debug.LogError("StatsManager requires GameManager on the same object.");
                return;
            }

            GameObject player = gameManager.PlayerCharacter;

            if (player == null)
            {
                Debug.LogError("GameManager PlayerCharacter is null.");
                return;
            }

            _characterController = player.GetComponent<PlayerCharacter>();
            _healthController = player.GetComponentInParent<HealthController>();
            _equipmentManager = player.GetComponentInParent<EquipmentManager>();

            if (_characterController != null)
            {
                _characterController.CanUseRope = canUseRope;
            }
        }
    }
}
