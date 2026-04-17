using System;
using System.Collections.Generic;
using System.Linq;
using MonkeyBusiness.Managers;
using UnityEngine;
using Sirenix.OdinInspector;

namespace MonkeyBusiness.Perks.PerkEffects
{
    [Serializable]
    public class WeaponDamageEffect : PerkEffectBase
    {
        // Represents a mapping between a projectile prefab and its damage multiplier
        [Serializable]
        public struct ProjectileMultiplier
        {
            // Reference to the projectile prefab this multiplier applies to
            [HorizontalGroup("Row", Width = 0.7f)]
            [HideLabel]
            public GameObject ProjectilePrefab;

            // Multiplier applied to the projectile's damage
            [HorizontalGroup("Row", Width = 0.3f)]
            [HideLabel]
            public float Multiplier;
        }

        // List of projectile-multiplier pairs configurable in the inspector
        [SerializeField]
        [ListDrawerSettings(Expanded = true)]
        private List<ProjectileMultiplier> _projectiles;

        // Applies the perk effect by setting damage multipliers for each projectile
        public override void Apply()
        {
            foreach (var entry in _projectiles)
            {
                // Skip invalid entries
                if (entry.ProjectilePrefab == null) continue;

                // Apply multiplier via StatsManager
                StatsManager.Instance.SetDamageMultiplier(entry.ProjectilePrefab, entry.Multiplier);
            }
        }

        // Resets all modified projectiles back to default multiplier (1x damage)
        public override void Reset()
        {
            foreach (var entry in _projectiles)
            {
                // Skip invalid entries
                if (entry.ProjectilePrefab == null) continue;

                // Reset multiplier to default value
                StatsManager.Instance.SetDamageMultiplier(entry.ProjectilePrefab, 1f);
            }
        }
        
        // Generates a formatted description string for UI display
        public override string GetDescription()
        {
            // Fallback to base description if no projectile data exists
            if (_projectiles == null || _projectiles.Count == 0)
                return description;

            // Build a multiline string like:
            // Fireball x2
            // Arrow x1.5
            string combined = string.Join("\n ",
                _projectiles
                    .Where(p => p.ProjectilePrefab != null) // Filter out invalid entries
                    .Select(p => $"{p.ProjectilePrefab.name} x{p.Multiplier}")
            );

            // Replace placeholder in description with generated data
            return description.Replace("<data>", combined);
        }
        
        // Tooltip shown in inspector explaining available placeholders
        protected override string GetTooltip()
        {
            return "Available placeholders:\n<data> - List of projectile multipliers (e.g. Fireball x2, Arrow x1.5)";
        }
    }
}