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
            _item.OnThrownCollision.AddListener(HandleCollision);
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void HandleCollision(GameObject other)
        {
            
            // TODO implement with new health system
            if (other.CompareTag("Enemy"))
            {
                // TODO
                HealthController enemyHealth = other.GetComponentInParent<HealthController>();
                enemyHealth.TakeDamage(_impactDamage);
            }
            
            Destroy(gameObject);
        }
    }
}
