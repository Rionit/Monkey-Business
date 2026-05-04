using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace MonkeyBusiness.Misc
{
    public class KnockbackController : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Components that will be suppressed during the knockback")]
        List<Behaviour> _suppressedComponents;

        [SerializeField]
        [Tooltip("The rigidbody that will be used for the knockback")]
        Rigidbody _body;

        [SerializeField]
        float _minimumVelocity = 0.1f;

        [ShowInInspector]
        [ReadOnly]
        bool _isKnockedback;

        public void Start()
        {
            _body.isKinematic = true;
        }

        [Button("Knockback with force", DisplayParameters = true)]
        public void Knockback(Vector3 force, float duration)
        {
            foreach (var behaviour in _suppressedComponents)
                behaviour.enabled = false;
            
            _body.isKinematic = false;

            _body.AddForce(force, ForceMode.VelocityChange);
            
            _isKnockedback = true;

            StartCoroutine(KnockbackCoroutine(duration));
        }

        IEnumerator KnockbackCoroutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            
            _body.linearVelocity = Vector3.zero;

            _isKnockedback = false;

            _body.isKinematic = true;

            foreach (var behaviour in _suppressedComponents)
                behaviour.enabled = true;
        }
    }
}
