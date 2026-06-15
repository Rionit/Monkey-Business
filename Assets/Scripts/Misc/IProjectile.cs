using UnityEngine;
using UnityEngine.Events;

namespace MonkeyBusiness.Combat.Weapons
{
    /// <summary>
    /// Interface describing basic properties of projectiles.
    /// </summary>
    public interface IProjectile
    {
        /// <summary>
        /// Damage dealt by the projectile on hit.
        /// </summary>
        public float Damage { get; set; }

        /// <summary>
        /// Multiplies the damage by this amount.
        /// </summary>
        public float DamageMultiplier { get; set; }

        /// <summary>
        /// Speed of the projectile. [Units/s]
        /// </summary>
        public float Speed { get; }

        /// <summary>
        /// Normalized direction of the projectile's movement.
        /// </summary>
        public Vector3 Direction { get; }

        public float MaxFlyDistance { get; }

        public float HitboxRadius { get; }

        /// <summary>
        /// Event invoked when a target is hit, passing the hit target as an argument.
        /// </summary>
        public UnityEvent<GameObject> OnTargetHit { get; }

        /// <summary>
        /// Mask of layers that can destroy the projectile on contact (e.g. walls, obstacles).
        /// </summary>
        LayerMask DestroyedBy { get; }
    }
}