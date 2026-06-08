using System;
using Ami.BroAudio;
using Ami.BroAudio.Runtime;
using UnityEngine;

namespace MonkeyBusiness.Misc
{
    public class RoomTone : MonoBehaviour
    {
        [SerializeField] private SoundSource source;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                source.Play();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                source.Pause(1f);
            }
        }
    }
}
