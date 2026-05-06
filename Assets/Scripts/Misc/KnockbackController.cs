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




        [SerializeField]
        [HideInInspector]

        float _defaultDamping = 1f;

        [ShowInInspector]
        float _currentDamping;

        [ShowInInspector]
        [Tooltip("Default damping of the knockback, in percent of current velocity.\n<i>Values under 90% usually result in an almost immediate stop</i>")]
        public float DefaultDamping
        {
            get => _defaultDamping * 100f;
            set
            {
                _defaultDamping = Mathf.Round(value) / 100f ;
            }
        }

        public void Start()
        {
            _body.isKinematic = true;
            _currentDamping = _defaultDamping;
        }

        public void FixedUpdate()
        {
            if(_isKnockedback)
            {
                _body.linearVelocity *= _currentDamping;
            }
        }

        [Button("Knockback with force", DisplayParameters = true)]
        public void Knockback(Vector3 force, float duration)
        {
            foreach (var behaviour in _suppressedComponents)
                behaviour.enabled = false;
            
            _body.isKinematic = false;

            Debug.Log("Applying knockback with force " + force + " for duration " + duration);

            _body.AddForce(force, ForceMode.Impulse);
            _currentDamping = _defaultDamping;
            _isKnockedback = true;

            StartCoroutine(KnockbackCoroutine(duration));
        }

        public void Knockback(Vector2 force, float duration)
        {
            var forcePower = force.magnitude;
            var normalizedForce = force.normalized;

            var force3d = new Vector3(normalizedForce.x, 0.5f, normalizedForce.y).normalized;


            _currentDamping = _defaultDamping;
            Knockback(force3d * forcePower, duration);
        }

        public void Knockback(Vector3 force, float duration, float damping)
        {
            foreach (var behaviour in _suppressedComponents)
                behaviour.enabled = false;
            
            _body.isKinematic = false;

            Debug.Log("Applying knockback with force " + force + " for duration " + duration);

            _body.AddForce(force, ForceMode.Impulse);
            _currentDamping = damping;
            _isKnockedback = true;

            StartCoroutine(KnockbackCoroutine(duration));

        }

        IEnumerator KnockbackCoroutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            
            _body.linearVelocity = Vector3.zero;

            _currentDamping = _defaultDamping;
            _isKnockedback = false;

            _body.isKinematic = true;

            foreach (var behaviour in _suppressedComponents)
                behaviour.enabled = true;
        }
    }
}
