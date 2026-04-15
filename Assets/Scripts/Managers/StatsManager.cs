using MonkeyBusiness.Combat.Health;
using UnityEngine;

namespace MonkeyBusiness.Managers
{
    public class StatsManager : MonoBehaviour
    {
        public static StatsManager Instance { get; private set; }

        public float PlayerMaxHealth
        {
            // Null check is only to avoid error in the Editor
            get => _healthController == null ? float.NaN : _healthController.MaxHealth;
            set
            {
                if (_healthController == null) return;
                _healthController.SetMaxHealth(value);
            }
        }
        
        private HealthController _healthController;
        private EquipmentManager _equipmentManager;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of StatsManager detected! Replacing the old one.");
            }
            Instance = this;

            var player = GameObject.FindGameObjectsWithTag("Player");
            _healthController = player[0].GetComponentInParent<HealthController>();
            _equipmentManager = player[0].GetComponentInParent<EquipmentManager>();
        }
    }
}
