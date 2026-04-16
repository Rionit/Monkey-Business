using MonkeyBusiness.Combat.Health;
using UnityEngine;

namespace MonkeyBusiness.Items
{
    [RequireComponent(typeof(Item))]
    /// <summary>
    /// A throwable object that heals the player when picked up and leaves behind a slippery banana peel when dropped
    /// </summary>
    public class Banana : MonoBehaviour
    {
        private Item _item;

        private bool _isEaten = false;

        private Transform _holder;

        [SerializeField]
        private int _healAmount = 20;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _item = GetComponent<Item>();

            _item.OnPickup.AddListener(HandlePickUp);
            _item.OnThrow.AddListener(HandleThrow);

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
    }
}
