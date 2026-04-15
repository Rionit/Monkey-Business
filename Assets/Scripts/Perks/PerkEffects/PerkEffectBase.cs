using System;
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
            
        // Applies the perk effect (implemented by subclasses)
        public abstract void Apply();
        
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
    }
}