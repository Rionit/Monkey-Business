using System;
using MonkeyBusiness.Items;
using UnityEngine;

namespace MonkeyBusiness.Perks.PerkEffects
{
    [Serializable]
    public class LongerMonksterEffect : PerkEffectBase
    {
        [SerializeField] private float frenzyDurationBoost;
        
        public override void Apply()
        {
            MonksterController.frenzyDurationOverride += frenzyDurationBoost;
        }

        public override void Update()
        {
            
        }

        public override void Reset()
        {
            MonksterController.frenzyDurationOverride -= frenzyDurationBoost;
        }
         
        // You can also override this function to return text with your values
        public override string GetDescription()
        {
            return description.Replace("<len>", (MonksterController.frenzyDurationOverride + frenzyDurationBoost).ToString());
        }
        
        // Don't forget to tell the designers what <placeholder> types they can use!
        // e.g. <multiplier>, <object> etc.
        protected override string GetTooltip()
        {
            return "Available placeholders:\n<len> - New length in seconds of Monkster frenzy";
        }

    }
}
