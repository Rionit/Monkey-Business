using System;
using MonkeyBusiness.Misc;
using UnityEngine;

namespace MonkeyBusiness.Items
{
    public class CollectiblePerkPickup : MonoBehaviour
    {
        [SerializeField] private StaticEvents.CollectiblePerkType perkType;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player" && !CollectiblePerkController.IsActive(perkType) && !CollectiblePerkController.isAnimating)
            {
                StaticEvents.OnCollectiblePerkPicked.Invoke(perkType);
                Destroy(gameObject);
            }
        }
    }
}
