using UnityEngine;

namespace MonkeyBusiness.Combat
{
    /// <summary>
    /// Contains stats and used ammo for a weapon.
    /// </summary>
    public class WeaponData : ScriptableObject
    {
        /// <summary>
        /// Maximum ammo the weapon can hold. If the weapon is not ammo-based, set to 0 or 1 (depending on whether you want to show ammo count in the UI or not).
        /// </summary>
        [field:SerializeField]
        public int MaxAmmo { get; set; }

        [field:SerializeField]
        public GameObject ProjectilePrefab { get; set; }

        /// <summary>
        /// Rate of fire, in bullets per <b>second</b>.
        /// </summary>
        [field:SerializeField]
        public float RateOfFire { get; set; }
    }
}