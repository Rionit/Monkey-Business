using System;
using UnityEngine;

namespace MonkeyBusiness.Player
{
    public class Teleport : MonoBehaviour
    {
        public Transform target;
        
        private void OnTriggerEnter(Collider other)
        {
            PlayerCharacter pc = other.gameObject.GetComponent<PlayerCharacter>();
            if (pc != null)
            {
                Debug.LogWarning("Teleport triggered: " + other);
                pc.SetPosition(target.position);
            }
        }
    }
}
