using System;
using MonkeyBusiness.Managers;
using UnityEngine;

namespace MonkeyBusiness.Perks.PerkEffects
{
    [Serializable]
    public class RopeSwingEffect : PerkEffectBase
    {
        [SerializeField] private bool canUse;
        
        public override void Apply()
        {
            StatsManager.Instance.canUseRope = canUse;
        }

        public override void Update()
        {
            
        }

        public override void Reset()
        {
            return;
        }
        
         
        // You can also override this function to return text with your values
        public override string GetDescription()
        {
            return description.Replace("<>", canUse.ToString());
        }
        
        // Don't forget to tell the designers what <placeholder> types they can use!
        // e.g. <multiplier>, <object> etc.
        protected override string GetTooltip()
        {
            return "Available placeholders:\n<value> - Max health change amount";
        }
    }
}
