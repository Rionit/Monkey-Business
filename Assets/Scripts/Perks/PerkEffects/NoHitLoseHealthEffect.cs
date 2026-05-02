using System;
using System.Collections;
using MonkeyBusiness.Managers;
using MonkeyBusiness.Misc;
using UnityEngine;

namespace MonkeyBusiness.Perks.PerkEffects
{
    /// <summary>
    /// This perk effect causes the player to lose health over time
    /// if they do not hit an enemy within a certain time window.
    /// </summary>
    [Serializable]
    public class NoHitLoseHealth : PerkEffectBase
    {
        [Header("Settings")]
        [SerializeField] private float damagePerTick = 1f;     // How much health to lose each tick
        [SerializeField] private float tickInterval = 1f;      // Time (in seconds) before applying damage
        [SerializeField] private float maxNoHitTime = 1f;      // Time threshold before damage starts

        // Internal timer tracking time since last enemy hit
        private float timeSinceLastHit = 0f;

        // Whether this effect is currently active
        private bool isActive = false;

        private Coroutine routine;

        /// <summary>
        /// Called when the perk is applied.
        /// Enables the effect and resets timer.
        /// </summary>
        public override void Apply()
        {
            isActive = true;
            timeSinceLastHit = 0f;

            // Subscribe to enemy hit event 
            EventManager.Instance.OnEnemyHit.AddListener(OnEnemyHit);

            routine = GameManager.Instance.StartCoroutine(TickRoutine());
        }

        public override void Update()
        {
            
        }

        /// <summary>
        /// Called when the perk is removed/reset.
        /// Disables the effect and unsubscribes from events.
        /// </summary>
        public override void Reset()
        {
            isActive = false;

            // Unsubscribe from enemy hit event
            EventManager.Instance.OnEnemyHit.RemoveListener(OnEnemyHit);

            if (routine != null)
            {
                GameManager.Instance.StopCoroutine(routine);
                routine = null;
            }
        }

        /// <summary>
        /// This method should be connected to the OnEnemyHit event.
        /// Resets the timer whenever player hits an enemy.
        /// </summary>
        public void OnEnemyHit(float damage)
        {
            timeSinceLastHit = 0f;
        }

        /// <summary>
        /// Coroutine replacement for Update/Tick logic.
        /// </summary>
        private IEnumerator TickRoutine()
        {
            while (isActive)
            {
                // Increase time since last hit
                timeSinceLastHit += Time.deltaTime;

                // If player hasn't hit anything for longer than allowed
                if (timeSinceLastHit >= maxNoHitTime)
                {
                    yield return new WaitForSeconds(tickInterval);

                    if (!isActive)
                        yield break;

                    // Reduce player health
                    StatsManager.Instance.PlayerHealth -= damagePerTick;

                    timeSinceLastHit = 0f;
                }
                else
                {
                    yield return null;
                }
            }
        }

        // Description override for UI
        public override string GetDescription()
        {
            return description
                .Replace("<damage>", damagePerTick.ToString())
                .Replace("<time>", maxNoHitTime.ToString());
        }

        protected override string GetTooltip()
        {
            return "Available placeholders:\n" +
                   "<damage> - Health lost per tick\n" +
                   "<time> - Time without hitting enemy before damage starts";
        }
    }
}