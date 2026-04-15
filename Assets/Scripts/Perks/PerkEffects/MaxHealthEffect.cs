using System;
using MonkeyBusiness.Managers;
using UnityEngine;

namespace MonkeyBusiness.Perks.PerkEffects
{
    [Serializable]
    public class MaxHealthEffect : PerkEffectBase
    {
        [SerializeField] private float value;
        
        public override void Apply()
        {
            StatsManager.Instance.PlayerMaxHealth += value;
        }

        public override void Reset()
        {
            StatsManager.Instance.PlayerMaxHealth -= value;
        }
        
        public override string GetDescription()
        {
            return description.Replace("<value>", value.ToString());
        }
        
        protected override string GetTooltip()
        {
            return "Available placeholders:\n<value> - Max health change amount";
        }
    }
}
