using System;
using System.Collections.Generic;
using MonkeyBusiness.Items;
using MonkeyBusiness.Managers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MonkeyBusiness.Perks.PerkEffects
{
    [Serializable]
    public class ThereAreNoItemsEffect : PerkEffectBase
    {
        [SerializeField] private Item.ItemType itemToRemove;
        
        public override void Apply()
        {
            var gameManager = GameManager.Instance;
            var items = new List<GameObject>(gameManager.GetItems());

            foreach (var go in items)
            {
                Item item = go.GetComponent<Item>();
                if (item.type != itemToRemove)
                    continue;
                
                Object.Destroy(go);
            }
            
            GameManager.Instance.StopItemSpawnThisWave();
        }

        public override void Update()
        {
            
        }

        public override void Reset()
        {
            
        }
         
        // You can also override this function to return text with your values
        public override string GetDescription()
        {
            return description.Replace("<item>", itemToRemove.ToString());
        }
        
        // Don't forget to tell the designers what <placeholder> types they can use!
        // e.g. <multiplier>, <object> etc.
        protected override string GetTooltip()
        {
            return "Available placeholders:\n<item> - The item that will be removed from the map for next wave";
        }

    }
}
