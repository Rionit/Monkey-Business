using System;
using MonkeyBusiness.Items;
using MonkeyBusiness.Managers;
using UnityEngine;

namespace MonkeyBusiness.Perks.PerkEffects
{
    [Serializable]
    public class LongerMonksterEffect : PerkEffectBase
    {
        [SerializeField] private float frenzyDurationBoost;
        
        public override void Apply()
        {
            StatsManager.Instance.MonksterFrenzyDuration += frenzyDurationBoost;
        }

        public override void Update()
        {
            
        }

        public override void Reset()
        {
            StatsManager.Instance.MonksterFrenzyDuration -= frenzyDurationBoost;
        }
         
        // You can also override this function to return text with your values
        public override string GetDescription()
        {
            return description.Replace("<len>", (StatsManager.Instance.MonksterFrenzyDuration + frenzyDurationBoost).ToString());
        }
        
        // Don't forget to tell the designers what <placeholder> types they can use!
        // e.g. <multiplier>, <object> etc.
        protected override string GetTooltip()
        {
            return "Available placeholders:\n<len> - New length in seconds of Monkster frenzy";
        }

    }
}
