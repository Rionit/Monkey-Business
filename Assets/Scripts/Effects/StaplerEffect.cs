using UnityEngine;
using MonkeyBusiness.Enemies;
using MonkeyBusiness.Combat.Weapons;

namespace MonkeyBusiness.Effects
{
    public class StaplerEffect : MonoBehaviour
    {
        [SerializeField]
        private ProjectileController _projectileController;

        
        [field:SerializeField]
        [Tooltip("Duration of the stapler effect in seconds.")]
        public float StaplerEffectDuration { get; set; } = 3f;

        [field:SerializeField]
        [Tooltip("Speed multiplier applied to the enemy when hit by the stapler effect.")]
        [Range(0.1f, 1f)]
        public float SpeedMultiplier { get; set; } = 0.8f;


        void InflictStaplerEffect(GameObject target)
        {
            var enemyFollowController = target.GetComponentInParent<EnemyFollowController>();
            if (enemyFollowController != null)
            {
                enemyFollowController.Slowdown(StaplerEffectDuration, SpeedMultiplier);
            }
        }

        void Awake()
        {
            if (_projectileController == null)
            {
                Debug.LogError("ProjectileController reference is missing on StaplerEffect!");
            }
            else
            {
                // Subscribes to the projectile's hit event
                _projectileController.OnTargetHit.AddListener(InflictStaplerEffect);
            }
        }

        void OnDestroy()
        {
            if (_projectileController != null)
            {
                _projectileController.OnTargetHit.RemoveListener(InflictStaplerEffect);
            }
        }

    }
}
