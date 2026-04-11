
using UnityEngine;

namespace MonkeyBusiness.Combat
{
    /// <summary>
    /// Interface for attack types including the basic features.
    /// </summary>
    public interface IAttack
    {
        /// <summary>
        /// Damage of the attack, in health units.
        /// </summary>
        public float Damage { get; }

        /// <summary>
        /// Time before the attack actually fires, in seconds.
        /// </summary>
        public float ChargeTime { get; }

        /// <summary>
        /// Method to execute the attack logic.
        /// </summary>
        void ExecuteAttack(GameObject target);
    }
}