using MonkeyBusiness.Combat.Health;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Sirenix.OdinInspector;

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

        [SerializeField]
        [FoldoutGroup("Debug")]
        bool _debugDrawColliders = false;

        [field:SerializeField]
        public UnityEvent<HealthController> OnTargetHit { get; private set; } = new UnityEvent<HealthController>();

        [ShowInInspector, ReadOnly]
        [Tooltip("Triggers used for the zone (= all colliders on this object)")]
        Collider[] _triggers;

        HashSet<HealthController> _alreadyHitTargets = new HashSet<HealthController>();

        void Awake()
        {
            GetColliders();
        }

        [Button("Get Colliders")]
        [Tooltip("Called automatically on Awake(), use in Editor for debugging reasons")]
        void GetColliders()
        {
            _triggers = GetComponents<Collider>();
            if(_triggers == null)
            {
                Debug.LogError("No collider found on ProximityWeaponZone " + gameObject.name);
            }
            else if(!_triggers[0].isTrigger)
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




        void OnDrawGizmos()
        {
            #if UNITY_EDITOR
            if(_debugDrawColliders && _triggers != null)
            {
                
                foreach(var trigger in _triggers)
                {
                    if(trigger is BoxCollider box)
                    {
                        Gizmos.color = new Color(1, 0, 0, 0.5f);
                        Gizmos.matrix = trigger.transform.localToWorldMatrix;
                        Gizmos.DrawCube(box.center, box.size);
                    }
                    else if(trigger is SphereCollider sphere)
                    {
                        Gizmos.color = new Color(1, 0, 0, 0.5f);
                        Gizmos.matrix = trigger.transform.localToWorldMatrix;
                        Gizmos.DrawSphere(sphere.center, sphere.radius);
                    }
                }
            }
            #endif
        }
    }

}