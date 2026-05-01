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
        
        [SerializeField]
        private float _chaseSpeed = 35.0f;

        /// <summary>
        /// Delay before the ball begins chasing target after bounce
        /// </summary>
        [SerializeField] private float _bounceChaseDelay = 0.15f;

        private float _chaseRotation = 5f;

        private float _chaseMagnitudeChange = 25f;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _item = GetComponent<Item>();
            _item.OnThrownCollision.AddListener(HandleCollision);
            _item.OnPickup.AddListener((_)=>{ClearChaseTarget();});
        }

        void FixedUpdate()
        {
            if(_chaseTarget != null)
            {
                Vector3 directionToTarget =  (_chaseTarget.transform.position - transform.position).normalized;

                Rigidbody _rigidbody = GetComponent<Rigidbody>();
                GetComponent<Rigidbody>().linearVelocity = Vector3.RotateTowards(GetComponent<Rigidbody>().linearVelocity, directionToTarget * _chaseSpeed, _chaseRotation * Time.fixedDeltaTime, _chaseMagnitudeChange * Time.fixedDeltaTime);
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

                GameObject nearestEnemy = null;
                float nearestEnemyDistance = Mathf.Infinity;

                foreach(Collider collider in colliders)
                {
                    Debug.Log(collider.gameObject.name);

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
                        nearestEnemy = collider.gameObject;
                        nearestEnemyDistance = distanceToEnemy;
                    }
                }

                if(nearestEnemy != null)
                {
                    Debug.Log($"Nearest enemy: {nearestEnemy}");
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
            

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.linearVelocity = 5 * Vector3.up;
            //rigidbody.AddForce(3 * Vector3.up, ForceMode.Impulse);
            
            _item.IsBeingThrown = true;
            
            //_chaseTarget = target;
            StartCoroutine(StartChaseAfterDelay(target));

            
        }

        private void ClearChaseTarget()
        {
            _chaseTarget = null;

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.useGravity = true;
        }

        IEnumerator StartChaseAfterDelay(Transform target)
        {
            yield return new WaitForSeconds(_bounceChaseDelay);
            _chaseTarget = target;
        }
    }
}
