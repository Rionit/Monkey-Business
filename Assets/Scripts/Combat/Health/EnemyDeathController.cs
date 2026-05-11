using System.Collections.Generic;
using DG.Tweening;
using System.Collections;
using UnityEngine;
using MonkeyBusiness.Misc;

namespace MonkeyBusiness.Combat.Health
{
    public class EnemyDeathController : MonoBehaviour
    {
        [SerializeField]
        float _preFadeoutDuration = 1f;

        [SerializeField]
        float _fadeoutDuration = 2f;
    
        [SerializeField]
        List<Behaviour> _componentsToDelete;


        [SerializeField]
        List<GameObject> _objectsToDelete;

        [SerializeField]
        float _deathImpulseForce = 35f;


        /// <summary>
        /// Alive collider that should be turned off
        /// </summary>
        Collider _aliveCollider;

        [SerializeField]
        [HideInInspector]
        EnemyTextureAnimator _deathTextureAnimator;

        [SerializeField]
        [Tooltip("Parent of the object's rig to be turned on")]
        GameObject _rigParent;

        Rigidbody _rigRB;

        void Awake()
        {
            _aliveCollider = GetComponent<Collider>();
            if(_aliveCollider == null)
            {
                Debug.LogError("No collider found on " + gameObject.name);
            }

            if(_rigParent == null)
            {
                Debug.LogError("No rig parent assigned for death fadeout on " + gameObject.name);
            }

            _rigRB = _rigParent.GetComponentInChildren<Rigidbody>();
            if(_rigRB == null)
            {
                Debug.LogError("No rigidbody found for death fadeout on " + gameObject.name);
            }

            _deathTextureAnimator = GetComponentInChildren<EnemyTextureAnimator>();
            if(_deathTextureAnimator == null)
            {
                Debug.LogError("No EnemyTextureAnimator found for death fadeout on " + gameObject.name);
            }
        }

        void Start()
        {
            var health = GetComponentInParent<HealthController>();
            if(health == null)
            {
                Debug.LogError("No health controller found for death fadeout on " + gameObject.name);
            }
        }

        public void StartDeathFadeout(Vector3 direction, Vector3 deathImpulse)
        {
            foreach(var component in _componentsToDelete)
            {
                if(component != null)
                {
                    Destroy(component);
                }
            }

            foreach(var obj in _objectsToDelete)
            {
                if(obj != null)
                {
                    Destroy(obj);
                }
            }

            _aliveCollider.enabled = false;

            ChangeLayerRecursively(transform, LayerMask.NameToLayer("DeadBody"));
        
            _rigRB.isKinematic = false;
            _rigRB.freezeRotation = false;

            _rigRB.maxLinearVelocity = _deathImpulseForce * 2f;
            _rigParent.SetActive(true);

            Debug.Log("Is the rig parent active? " + _rigParent.activeSelf);

            _rigRB.AddForce(deathImpulse + Vector3.up * 2f, ForceMode.Impulse);

            var modifiedDir = new Vector3(direction.z, 0f, -direction.x).normalized;
            _rigRB.AddTorque(modifiedDir * _deathImpulseForce / 4f, ForceMode.Impulse);

            StartCoroutine(FadeoutDestroyCoroutine());
        }

        public void StartDeathFadeout(Vector3 direction)
        {
            foreach(var component in _componentsToDelete)
            {
                if(component != null)
                {
                    Destroy(component);
                }
            }

            foreach(var obj in _objectsToDelete)
            {
                if(obj != null)
                {
                    Destroy(obj);
                }
            }

            _aliveCollider.enabled = false;

            ChangeLayerRecursively(transform, LayerMask.NameToLayer("DeadBody"));

            _rigRB.isKinematic = false;
            _rigRB.freezeRotation = false;

            _rigRB.maxLinearVelocity = _deathImpulseForce * 2f;

            _rigRB.linearVelocity = Vector3.zero;
            _rigRB.angularVelocity = Vector3.zero;

            _rigParent.SetActive(true);

            Debug.Log("Is rig parent active? " + _rigParent.activeSelf);

            var randAngle = Random.Range(0f, Mathf.PI * 2f);
            var randDirection = new Vector3(Mathf.Cos(randAngle), 0.3f, Mathf.Sin(randAngle)).normalized;

            _rigRB.AddForce(randDirection * _deathImpulseForce, ForceMode.Impulse);

            var modifiedDir = new Vector3(direction.z, 0f, -direction.x).normalized;
            _rigRB.AddTorque(modifiedDir * _deathImpulseForce / 4f, ForceMode.Impulse);
            //_rb.AddTorque(Random.insideUnitSphere * _deathImpulseForce, ForceMode.Impulse);
            //_rb.AddTorque(direction * _deathImpulseForce, ForceMode.Impulse);
            StartCoroutine(FadeoutDestroyCoroutine());
        }

        IEnumerator FadeoutDestroyCoroutine()
        {
            yield return _deathTextureAnimator.AnimateDeath();
            
            Destroy(gameObject);
        }

        void ChangeLayerRecursively(Transform obj, int newLayer)
        {
            obj.gameObject.layer = newLayer;
            foreach(Transform child in obj)
            {
                ChangeLayerRecursively(child, newLayer);
            }
        }
    }
}