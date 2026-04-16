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

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _item = GetComponent<Item>();
            _item.OnThrownCollision.AddListener(HandleCollision);
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void HandleCollision(GameObject other)
        {
            // TODO bounce on hitting an enemy

            if (other.CompareTag("Enemy"))
            {
                HealthController enemyHealth = other.GetComponentInParent<HealthController>();
                enemyHealth.TakeDamage(_impactDamage);
            }
            
            // Lower durability
            if(--_durability == 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
