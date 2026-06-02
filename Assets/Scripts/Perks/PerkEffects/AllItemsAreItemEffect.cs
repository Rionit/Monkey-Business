using System;
using System.Collections.Generic;
using MonkeyBusiness.Items;
using MonkeyBusiness.Managers;
using UnityEngine;

namespace MonkeyBusiness.Perks.PerkEffects
{
    [Serializable]
    public class AllItemsAreItemEffect : PerkEffectBase
    {
        [SerializeField] private GameObject itemToChangeTo;
        
        public override void Apply()
        {
            if (itemToChangeTo == null)
            {
                Debug.LogWarning("AllItemsAreItemEffect: itemToChangeTo is null.");
                return;
            }

            var gameManager = GameManager.Instance;
            var items = new List<GameObject>(gameManager.GetItems());

            foreach (var oldItem in items)
            {
                if (oldItem == null)
                {
                    continue;
                }

                Transform oldTransform = oldItem.transform;

                Vector3 position = oldTransform.position;
                Quaternion rotation = oldTransform.rotation;
                Transform parent = oldTransform.parent;

                GameObject newItem = GameObject.Instantiate(
                    itemToChangeTo,
                    position,
                    rotation,
                    parent
                );

                gameManager.RemoveItem(oldItem);
                GameObject.Destroy(oldItem);
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
            return description.Replace("<item>", itemToChangeTo.ToString());
        }
        
        // Don't forget to tell the designers what <placeholder> types they can use!
        // e.g. <multiplier>, <object> etc.
        protected override string GetTooltip()
        {
            return "Available placeholders:\n<item> - The item that all of the items will change to";
        }

    }
}
