using System;
using UnityEngine;

namespace MonkeyBusiness.Perks
{
    [Serializable]
    public class PerkEffectTemplate : PerkEffectBase
    {
        // You can add stuff here too for example:
        // [SerializeField] private float value;
        
        public override void Apply()
        {
            Debug.LogError("You forgot to apply perk effect!");
            /*
                EXAMPLE:
                StatsManager.Instance.PlayerMaxHealth += value;
             */
        }
        
        public override void Reset()
        {
            Debug.LogError("You forgot to reset the effect!");
            /*
                EXAMPLE:
                StatsManager.Instance.PlayerMaxHealth -= value;
            */
        }
        /*
         
        // You can also override this function to return text with your values
        public override string GetDescription()
        {
            return description.Replace("<value>", value.ToString());
        }
        
        // Don't forget to tell the designers what <placeholder> types they can use!
        // e.g. <multiplier>, <object> etc.
        protected override string GetTooltip()
        {
            return "Available placeholders:\n<value> - Max health change amount";
        }
        
        */
    }
}
