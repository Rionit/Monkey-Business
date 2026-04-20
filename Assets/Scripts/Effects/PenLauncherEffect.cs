using UnityEngine;
using Sirenix.OdinInspector;
using MonkeyBusiness.Combat.Weapons;
using Sirenix.Serialization;

namespace MonkeyBusiness.Effects
{
    public class PenLauncherEffect : MonoBehaviour
    {
        [Required]
        PlayerProjectileController _projectileController;
        
        [field:SerializeField]
        [Tooltip("Damage multiplier applied to the projectile for each consecutive hit on the same enemy.")]
        public float DamageModifierPerHit { get; set; } = 0.8f;

        void Awake()
        {
            if(_projectileController == null)
            {
                Debug.LogError("ProjectileController reference is missing on PenLauncherEffect!");
            }
            else
            {
                // Subscribes to the projectile's hit event
                _projectileController.OnTargetHit.AddListener(ReduceDamageOnHit);
            }
        }

        void ReduceDamageOnHit(GameObject _)
        {
            _projectileController.Damage *= DamageModifierPerHit;
        }
    }
}
