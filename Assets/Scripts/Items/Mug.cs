using UnityEngine;
using MonkeyBusiness.Combat.Health;

namespace MonkeyBusiness.Items
{
    [RequireComponent(typeof(Item))]
    public class Mug : MonoBehaviour
    {
        [SerializeField]
        private int _impactDamage = 35;

        private Item _item;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _item = GetComponent<Item>();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void OnCollisionEnter(Collision collision)
        {
            
            if (!_item.IsBeingThrown)
            {
                return;            
            }
            
            //Debug.Log(collision.gameObject.name);
            

            // TODO implement with new health system
            if (collision.gameObject.CompareTag("Enemy"))
            {
                // TODO
                HealthController enemyHealth = collision.gameObject.GetComponent<HealthController>();
                enemyHealth.TakeDamage(_impactDamage);
            }
            
            
            // Prevent dealing damage multiple times per throw
            _item.IsBeingThrown = false;

            // Re-enable collision with whoever threw this item
            Physics.IgnoreCollision(_item.ignoreCollision, GetComponent<Collider>(), false);
            

            Destroy(gameObject);
        }
    }
}
