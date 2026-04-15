using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MonkeyBusiness.Perks
{
    [Serializable]
    public abstract class PerkEffectBase
    {
        [LabelText("Perk Effect Description")]
        [Tooltip("@GetTooltip()")]
        [TextArea, ShowInInspector, SerializeField]
        protected string description;
            
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
