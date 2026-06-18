using System;
using MonkeyBusiness.Managers;
using UnityEngine;

namespace MonkeyBusiness.Perks.PerkEffects
{
    [Serializable]
    public class MaxHealthEffect : PerkEffectBase
    {
        [SerializeField] private float value;
        
        private float halvedValue => halfValue ? value * Mathf.Pow(0.5f, GetUsages()) : value;
        private float cachedValue;
        private bool activated = false;
                                     
        public override void Apply()
        {
            activated = true;
            cachedValue = halvedValue;
            StatsManager.Instance.PlayerMaxHealth += halvedValue;
        }
        
        public override void Update()
        {
        }

        public override void Reset()
        {
            activated = false;
            StatsManager.Instance.PlayerMaxHealth -= halvedValue;
        }
        
        public override string GetDescription()
        {
            return description.Replace("<value>", activated ? cachedValue.ToString() : halvedValue.ToString());
        }
        
        protected override string GetTooltip()
        {
            return "Available placeholders:\n<value> - Max health change amount";
        }
    }
}
