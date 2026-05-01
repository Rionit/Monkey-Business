using System.Collections;
using MonkeyBusiness.Combat.Health;
using UnityEngine;

namespace MonkeyBusiness.Items
{
    /// <summary>
    /// A throwable object that deals damage and can be thrown a few times before bursting
    /// 
    /// TODO implement bouncing towards and chasing enemies like a homing missile
    /// </summary>
    [RequireComponent(typeof(Item))]
    public class Basketball : MonoBehaviour
    {
        [SerializeField]
        private int _impactDamage = 20;

        private Item _item;

        /// <summary>
        /// Number of times the item can be thrown before breaking. Each throw removes 1 durability
        /// </summary>
        [SerializeField]
        private int _durability = 3;

        private float _bounceRadius = 20.0f;

        private Transform _chaseTarget;
        
        /// <summary>
        /// To stop the ball from targeting some enemies' feet
        /// </summary>
        private float _chaseTargetVerticalOffset;

        [SerializeField] private float _chaseSpeed = 35.0f;
        
        private Rigidbody _rigidbody;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _item = GetComponent<Item>();
            _item.OnThrownCollision.AddListener(HandleCollision);
            _item.OnPickup.AddListener((_)=>{ClearChaseTarget();});
            _rigidbody = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            if(_chaseTarget is not null)
            {
                Vector3 directionToTarget =  (_chaseTarget.transform.position + _chaseTargetVerticalOffset * Vector3.up - transform.position).normalized;

                
                _rigidbody.linearVelocity = directionToTarget * _chaseSpeed;
            }
        }

        void HandleCollision(GameObject other)
        {
            
            ClearChaseTarget();

            if (other.CompareTag("Enemy"))
            {
                HealthController enemyHealth = other.GetComponentInParent<HealthController>();
                enemyHealth.TakeDamage(_impactDamage);


                // Find next target to bounce to
                Collider[] colliders = Physics.OverlapSphere(transform.position, _bounceRadius, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);

                Collider nearestEnemy = null;
                float nearestEnemyDistance = Mathf.Infinity;

                foreach(Collider collider in colliders)
                {
                    //Debug.Log(collider.gameObject.name);

                    if (!collider.gameObject.CompareTag("Enemy"))
                    {
                        continue;
                    }

                    if(collider.transform.root.gameObject == other)
                    {
                        continue;
                    }

                    if(Physics.Linecast(transform.position, collider.transform.position, LayerMask.GetMask("Navigation")))
                    {
                        // object is obstructed by environment
                        continue;
                    }
                    
                    float distanceToEnemy = Vector3.Distance(transform.position, collider.transform.position);
                    if(distanceToEnemy < nearestEnemyDistance)
                    {
                        nearestEnemy = collider;
                        nearestEnemyDistance = distanceToEnemy;
                    }
                }

                if(nearestEnemy is not null)
                {
                    Debug.Log($"Nearest enemy: {nearestEnemy}");
                    Debug.Log($"Nearest enemy position: {nearestEnemy.transform.position}");
                    _rigidbody.linearVelocity = Vector3.zero;

                    // If we add more collider types to enemies such as boxes, add those here
                    if (nearestEnemy is CapsuleCollider capsuleCollider)
                    {
                        _chaseTargetVerticalOffset = capsuleCollider.center.y;
                    }
                    else
                    {
                        _chaseTargetVerticalOffset = 0.0f;
                    }
                    
                    
                    SetChaseTarget(nearestEnemy.transform);
                }
            }
            
            // Lower durability
            if(--_durability == 0)
            {
                Destroy(gameObject);
            }
        }
        

        private void SetChaseTarget(Transform target)
        {
            
            _rigidbody.useGravity = false;
            //rigidbody.AddForce(3 * Vector3.up, ForceMode.Impulse);
            
            _item.IsBeingThrown = true;
            
            _chaseTarget = target;
            
        }

        private void ClearChaseTarget()
        {
            _chaseTarget = null;

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.useGravity = true;
        }
    }
}
