using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonkeyBusiness.Perks
{
    [Serializable]
    public abstract class PerkEffectBase
    {
        [BoxGroup("Hidden Outcome (Revealed After Pick)")]
        [LabelText("Perk Effect Description")]
        [Tooltip("@GetTooltip()")]
        [TextArea, ShowInInspector]
        protected string description;
            
        //[ShowInInspector, ReadOnly]
        //private string Preview => GetDescription();

        [LabelText("Perk Effect Type")]
        [Tooltip("What type of effect it is or 'What does it modify?', e.g.: Health, Speed, Damage, etc.")]
        public PerkEffectType type;
        
        public abstract void Apply();
        
        public abstract void Reset();
        
        public virtual string GetDescription()
        {
            return description;
        }
        
        protected virtual string GetTooltip()
        {
            return "Available placeholders: (none)";
        }
    }
}
