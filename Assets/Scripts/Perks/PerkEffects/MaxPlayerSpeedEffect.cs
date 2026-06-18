using System;
using MonkeyBusiness.Managers;
using UnityEngine;

namespace MonkeyBusiness.Perks.PerkEffects
{
    [Serializable]
    public class MaxPlayerSpeedEffect : PerkEffectBase
    {
        [SerializeField] private float value;
        
        private float halvedValue => value * Mathf.Pow(0.5f, GetUsages());
        
        public override void Apply()
        {
            StatsManager.Instance.PlayerWalkSpeed += halvedValue;
        }
        
        public override void Update()
        {
        }

        public override void Reset()
        {
            StatsManager.Instance.PlayerWalkSpeed -= halvedValue;
        }
        
        public override string GetDescription()
        {
            return description.Replace("<value>", halvedValue.ToString());
        }
        
        protected override string GetTooltip()
        {
            return "Available placeholders:\n<value> - Max health change amount";
        }
    }
}
