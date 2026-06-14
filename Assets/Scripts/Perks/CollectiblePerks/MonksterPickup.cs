using System;
using MonkeyBusiness.Misc;
using UnityEngine;

namespace MonkeyBusiness.Items
{
    public class MonksterPickup : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player" && !MonksterController.isActive)
            {
                StaticEvents.OnMonksterPicked.Invoke();
                Destroy(gameObject);
            }
        }
    }
}
