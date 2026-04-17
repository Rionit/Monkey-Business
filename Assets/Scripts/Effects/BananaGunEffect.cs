using UnityEngine;
using MonkeyBusiness.Combat.Weapons;
using MonkeyBusiness.Combat.Health;
using System.Collections;

namespace MonkeyBusiness.Effects
{
    public class BananaGunEffect : MonoBehaviour
    {
        [SerializeField]
        private ProjectileController _projectileController;

        [field:SerializeField]
        [Tooltip("Duration of the banana poison effect in seconds.")]
        public float BananaPoisonDuration { get; set; } = 3f;

        [field:SerializeField]
        [Tooltip("Damage dealt per tick of the banana poison effect.")]
        public float PoisonTickDamage { get; set; } = 1;

        [field:SerializeField]
        [Tooltip("Number of ticks")]
        [field: Range(1, 100)]
        public int NumberOfTicks { get; set; } = 10;

        void InflictPoison(GameObject target)
        {
            var targetHealth = target.GetComponentInParent<HealthController>();
            if (targetHealth != null)
            {
                float tickInterval = BananaPoisonDuration / NumberOfTicks;
                targetHealth.ApplyPoison(PoisonTickDamage, tickInterval, NumberOfTicks); 
            }
        }

        void Awake()
        {
            if (_projectileController == null)
            {
                Debug.LogError("ProjectileController reference is missing on BananaGunEffect!");
            }
            else
            {
                // Subscribes to the projectile's hit event
                _projectileController.OnTargetHit.AddListener(InflictPoison);
            }
        }

        void OnDestroy()
        {
            if (_projectileController != null)
            {
                _projectileController.OnTargetHit.RemoveListener(InflictPoison);
            }
        }
    }
}
