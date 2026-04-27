using System.Collections.Generic;
using UnityEngine;
using MonkeyBusiness.Enemies.Navigation;
using MonkeyBusiness.Misc;
using MonkeyBusiness.Combat.Health;

namespace MonkeyBusiness.Items
{
    /// <summary>
    /// Thrown item that pierces through hit enemies and applies a movement slow. May be picked up and reused
    /// </summary>
    [RequireComponent(typeof(Item))]
    public class Rake : MonoBehaviour
    {
        private Item _item;

        [SerializeField]
        private Collider _mainCollider;

        [SerializeField]
        private Collider _pickupCollider;

        [SerializeField]
        private float _onHitSpeedModifier = 0.2f;
        [SerializeField]
        private float _slowDuration = 2.0f;

        [SerializeField]
        private float _impactDamage = 150;
        private List<Transform> _enemiesHitSinceLastThrow = new();

        private Rigidbody _rigidbody;

        /// <summary>
        /// The amount of times this item can be picked up and thrown before it breaks
        /// </summary>
        private int _durability = 2;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _item = GetComponent<Item>();
            _item.OnThrow.AddListener(HandleBeingThrown);

            _item.OnThrownCollision.AddListener(HandleThrowncollision);

            _item.OnPickup.AddListener(OnPickup);

            _rigidbody = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        void FixedUpdate()
        {
            if (_item.IsBeingThrown && _rigidbody.linearVelocity.magnitude > 0.1)
            {
                transform.rotation = Quaternion.LookRotation(_rigidbody.linearVelocity.normalized);
            }
        }
        
        void OnTriggerEnter(Collider collider)
        {
            if (!_item.IsBeingThrown)
            {
                return;
            }

            if (collider.gameObject.CompareTag("Enemy"))
            {
                if (!_enemiesHitSinceLastThrow.Contains(collider.transform.root))
                {
                    EnemyFollowController enemyFollowController = collider.gameObject.GetComponentInParent<EnemyFollowController>();
                    if(enemyFollowController != null)
                    {
                        enemyFollowController.Slowdown(_slowDuration, _onHitSpeedModifier);
                    }
                   
                    collider.gameObject.GetComponentInParent<HealthController>().TakeDamage(_impactDamage);
                    _enemiesHitSinceLastThrow.Add(collider.transform.root);
                }
            }
            else
            {
                // we hit a non-enemy
                _mainCollider.isTrigger = false;
                _pickupCollider.enabled = true;

                _rigidbody.linearVelocity = Vector3.zero;

            }
        }

        void HandleBeingThrown()
        {
            _mainCollider.isTrigger = true;
            _pickupCollider.enabled = false;

            _enemiesHitSinceLastThrow.Clear();

        }

        void OnPickup(Transform parent)
        {
            transform.rotation = Quaternion.LookRotation(parent.forward);
        }


        void HandleThrowncollision(GameObject other)
        {
            if(--_durability <= 0)
            {
                Destroy(gameObject);
            }
        }

    }
}
