using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace MonkeyBusiness.Misc
{
    public class KnockbackController : MonoBehaviour
    {

        [SerializeField]
        [Tooltip("The agent that will be suppressed during the knockback")]
        NavMeshAgent _agentOverride;

        [SerializeField]
        [Tooltip("The rigidbody that will be used for the knockback")]
        Rigidbody _body;

        [SerializeField]
        float _minimumVelocity = 0.1f;

        [ShowInInspector]
        [ReadOnly]
        bool _isKnockedback;

        [Button("Knockback with force", DisplayParameters = true)]
        public void Knockback(Vector3 force, float duration)
        {
            _body.AddForce(force, ForceMode.VelocityChange);
            _agentOverride.enabled = false;
            _isKnockedback = true;

            StartCoroutine(KnockbackCoroutine(duration));
        }

        IEnumerator KnockbackCoroutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            
            _isKnockedback = false;
            _agentOverride.enabled = true;
        }
    }
}
