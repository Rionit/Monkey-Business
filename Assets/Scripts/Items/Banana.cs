using MonkeyBusiness.Combat.Health;
using MonkeyBusiness.Misc;
using NUnit.Framework;
using UnityEngine;

namespace MonkeyBusiness.Items
{
    [RequireComponent(typeof(Item))]
    /// <summary>
    /// A throwable object that heals the player when picked up and leaves behind a slippery banana peel when dropped
    /// 
    /// TODO: Currently only stuns on direct impact when thrown
    /// TODO: Implement trap mechanic
    /// </summary>
    public class Banana : MonoBehaviour
    {
        private Item _item;

        /// <summary>
        /// Has the banana been eaten
        /// </summary>
        private bool _isEaten = false;

        /// <summary>
        /// If true, the banana acts as a trap.
        /// </summary>
        private bool _bananaPeelPrimed = false;

        private Transform _holder;

/// <summary>
/// How much this item heals when eaten
/// </summary>
        [SerializeField]
        private int _healAmount = 20;

/// <summary>
/// How long the banana peel stuns for when stepped on
/// </summary>
        [SerializeField]
        private float _stunDuration = 1.0f;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _item = GetComponent<Item>();

            _item.OnPickup.AddListener(HandlePickUp);
            _item.OnThrow.AddListener(HandleThrow);
            _item.OnThrownCollision.AddListener(HandleCollision);

            _item.KeepAfterThrowing = true;

        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void HandleThrow()
        {
            if (_isEaten)
            {
                HandleSecondThrow();
            }
            else
            {
                HandleFirstThrow();
            }
        }

        void HandleFirstThrow()
        {
            Debug.Log("Eating a banana, yummers!");

            _holder.gameObject.GetComponentInParent<HealthController>().Heal(_healAmount);

            _isEaten = true;
            _item.PickUp(_holder);
        }

        void HandleSecondThrow()
        {
            // The way the order of execution is, this will be checked in the manager after this method is finished
            _item.KeepAfterThrowing = false;
            Debug.Log("Throwing the banana peel and littering all over the place tbh");
        }

        void HandlePickUp(Transform parent)
        {
            _holder = parent;
        }

        void HandleCollision(GameObject other)
        {
            if (!_isEaten)
            {
                return;
            }
            StunController stunController = other.GetComponentInParent<StunController>();
            if(stunController)
            {
                stunController.Stun(_stunDuration);
                Destroy(gameObject);
            }           
        }

/// <summary>
/// Trigger only used when in banana peel form
/// </summary>
/// <param name="other"></param>
        void OnTriggerEnter(Collider other)
        {
            if(!_bananaPeelPrimed)
            {
                return;
            }

            HandleCollision(other.gameObject);
        }


        /// <summary>
        /// This method only executes when eaten and dropped/thrown.
        /// Sets up this item as a trap.
        /// </summary>
        /// <param name="collision"></param>
        void OnCollisionEnter(Collision collision)
        {
            if(!_isEaten || _item.isBeingHeld)
            {
                return;
            }

            // make banana peel stick
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            rigidbody.freezeRotation = true;
            rigidbody.linearVelocity = Vector3.zero;

            transform.rotation = Quaternion.identity;

            // Change state from thrown/dropped to active peel trap
            _bananaPeelPrimed = true;
            // Remove Item tag from this gameobject to prevent it from being picked up again by the player
            gameObject.tag = "Untagged";
        }
    }
}
