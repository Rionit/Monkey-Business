using MonkeyBusiness.Combat.Health;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace MonkeyBusiness.Combat.Weapons
{

    using Camera = UnityEngine.Camera;
    public class ProximityWeaponZone : MonoBehaviour
    {

        [SerializeField]
        string _targetTag = "Enemy";
        
        [SerializeField]
        [Tooltip("Whether the weapon should hit targets through walls. If false, the weapon will only hit targets that are directly reachable from the center of the hitbox.")]
        bool _hitsThroughWalls = true;

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
                Debug.Log("Has tag");
                HealthController healthController = other.GetComponentInParent<HealthController>();

                if(healthController != null)
                {
                    if(!_alreadyHitTargets.Contains(healthController))
                    {
                        if(!_hitsThroughWalls)
                        {
                            var raycastPos = Camera.main.transform.position;
                            var direction = (other.transform.position - raycastPos).normalized;
                            var raycastDistance = Vector3.Distance(raycastPos, other.transform.position);
                            if(Physics.Raycast(raycastPos, direction, out RaycastHit hitInfo, raycastDistance, LayerMask.GetMask("Default", "Navigation")))
                            {
                                if(hitInfo.collider != other)
                                {
                                    Debug.Log("Shotgun hit " + other.name + " but it was blocked by " + hitInfo.collider.name);
                                    return;
                                }
                                else
                                {
                                    _alreadyHitTargets.Add(healthController);
                                    OnTargetHit.Invoke(healthController);
                                }
                            }
                        }
                        else
                        {
                            _alreadyHitTargets.Add(healthController);
                            OnTargetHit.Invoke(healthController);
                        }
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