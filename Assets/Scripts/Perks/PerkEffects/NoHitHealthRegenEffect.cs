using System;
using System.Collections;
using MonkeyBusiness.Managers;
using MonkeyBusiness.Misc;
using UnityEngine;

namespace MonkeyBusiness.Perks.PerkEffects
{
    /// <summary>
    /// This perk effect causes the player to gain health over time
    /// if they do not hit an enemy within a certain time window.
    /// </summary>
    [Serializable]
    public class NoHitHealthRegen : PerkEffectBase
    {
        [Header("Settings")]
        [SerializeField] private float healthPerTick = 1f;     // How much health to gain each tick
        [SerializeField] private float tickInterval = 1f;      // Time (in seconds) between ticks
        [SerializeField] private float maxNoHitTime = 1f;      // Time threshold before regen starts

        private float timeSinceLastHit = 0f;
        private bool isActive = false;
        private Coroutine routine;

        public override void Apply()
        {
            isActive = true;
            timeSinceLastHit = 0f;

            EventManager.Instance.OnEnemyHit.AddListener(OnEnemyHit);
            routine = GameManager.Instance.StartCoroutine(TickRoutine());
        }

        public override void Update()
        {
        }

        public override void Reset()
        {
            isActive = false;

            EventManager.Instance.OnEnemyHit.RemoveListener(OnEnemyHit);

            if (routine != null)
            {
                GameManager.Instance.StopCoroutine(routine);
                routine = null;
            }
        }

        public void OnEnemyHit(float damage)
        {
            timeSinceLastHit = 0f;
        }

        private IEnumerator TickRoutine()
        {
            while (isActive)
            {
                timeSinceLastHit += Time.deltaTime;

                if (timeSinceLastHit >= maxNoHitTime)
                {
                    yield return new WaitForSeconds(tickInterval);

                    if (!isActive)
                        yield break;

                    // Increase player health
                    StatsManager.Instance.PlayerHealth += healthPerTick;

                    timeSinceLastHit = 0f;
                }
                else
                {
                    yield return null;
                }
            }
        }

        public override string GetDescription()
        {
            return description
                .Replace("<heal>", healthPerTick.ToString())
                .Replace("<time>", maxNoHitTime.ToString());
        }

        protected override string GetTooltip()
        {
            return "Available placeholders:\n" +
                   "<heal> - Health gained per tick\n" +
                   "<time> - Time without hitting enemy before regen starts";
        }
    }
}