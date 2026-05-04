using MonkeyBusiness.Combat.Health;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace MonkeyBusiness.Combat.Weapons
{
    public class ProximityWeaponZone : MonoBehaviour
    {

        [SerializeField]
        string _targetTag = "Enemy";

        [field:SerializeField]
        public UnityEvent<HealthController> OnTargetHit { get; private set; } = new UnityEvent<HealthController>();

        Collider _trigger;

        HashSet<HealthController> _alreadyHitTargets = new HashSet<HealthController>();

        void Awake()
        {
            _trigger = GetComponent<Collider>();
            if(_trigger == null)
            {
                Debug.LogError("No collider found on ProximityWeaponZone " + gameObject.name);
            }
            else if(!_trigger.isTrigger)
            {
                Debug.LogError("Collider on ProximityWeaponZone " + gameObject.name + " is not set as trigger!");
            }
        }
    
        void OnEnable()
        {
            _alreadyHitTargets.Clear();
        }

        void OnTriggerEnter(Collider other)
        {
            Debug.Log("Entering trigger with " + other.name);
            if(!enabled) Debug.LogError("Receiving trigger events while disabled!");
            if(other.CompareTag(_targetTag))
            {
                HealthController healthController = other.GetComponentInParent<HealthController>();

                if(healthController != null)
                {
                    if(!_alreadyHitTargets.Contains(healthController))
                    {
                        _alreadyHitTargets.Add(healthController);
                        OnTargetHit.Invoke(healthController);
                    }
                }
                else
                {
                    Debug.LogWarning("Object " + other.name + " has tag " + _targetTag + " but no HealthController found in parents.");
                }
            }
        }
    }

}