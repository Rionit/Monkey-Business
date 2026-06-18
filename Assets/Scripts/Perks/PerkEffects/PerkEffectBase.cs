using System;
using MonkeyBusiness.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonkeyBusiness.Perks.PerkEffects
{
    [Serializable]
    public abstract class PerkEffectBase
    {
        // Base description used for UI, supports placeholder replacement
        [LabelText("Perk Effect Description")]
        [Tooltip("@GetTooltip()")] // Dynamically fetch tooltip from derived classes
        [TextArea, ShowInInspector, SerializeField]
        protected string description;
        
        [NonSerialized]
        protected ScriptableObject perkSO;

        [NonSerialized]
        protected bool halfValue;
            
        public void Initialize(ScriptableObject owner, bool _halfValue)
        {
            halfValue = _halfValue;
            perkSO = owner;
        }
        
        // Applies the perk effect (implemented by subclasses)
        public abstract void Apply();

        // Call this from Perk in its Update if you need to change stuff dynamically
        public abstract void Update();
        
        // Reverts the perk effect (implemented by subclasses)
        public abstract void Reset();
        
        // Returns the final description (can be overridden for dynamic content)
        public virtual string GetDescription()
        {
            return description;
        }
        
        // Provides tooltip text for inspector (can be overridden)
        protected virtual string GetTooltip()
        {
            return "Available placeholders: (none)";
        }

        public virtual int GetUsages()
        {
            return StatsManager.Instance._perksUsage.TryGetValue(perkSO, out var count)
                ? count
                : 0;
        }
    }
}