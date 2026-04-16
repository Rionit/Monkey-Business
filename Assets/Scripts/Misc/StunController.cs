using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace MonkeyBusiness.Misc
{
    public class StunController : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Components that are gonna be turned off for the stun duration")]
        List<Behaviour> _stunnedComponents; 

        /// <summary>
        /// Stuns the components for the specified duration.
        /// </summary>
        /// <param name="duration"></param>
        [BoxGroup("Debug")]
        [Button("Stun for seconds", DisplayParameters = true)]
        public void Stun(float duration)
        {
            StartCoroutine(StunCoroutine(duration));
        }

        IEnumerator StunCoroutine(float duration)
        {
            Debug.Log("Stunned for " + duration + " seconds!");
            foreach (var component in _stunnedComponents)
            {
                if (component != null)
                    component.enabled = false;
            }

            yield return new WaitForSeconds(duration);

            foreach (var component in _stunnedComponents)
            {
                if (component != null)
                    component.enabled = true;
            }
        }
    }
}
