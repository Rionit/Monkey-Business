using MonkeyBusiness.Combat.Health;
using MonkeyBusiness.Misc;
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

        private bool _isEaten = false;

        private Transform _holder;

        [SerializeField]
        private int _healAmount = 20;

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
            }
            // TODO add ganana peel remaining on the ground
            Destroy(gameObject);
        }
    }
}
