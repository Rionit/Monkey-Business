using UnityEngine;
using MonkeyBusiness.Combat.Health;

namespace MonkeyBusiness.Items
{
    /// <summary>
    /// A throwable item that shatters after being thrown
    /// </summary>
    public class Mug : MonoBehaviour
    {
        [SerializeField]
        private int _impactDamage = 35;

        private Item _item;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _item = GetComponentInChildren<Item>();
            _item.OnThrownCollision.AddListener(HandleCollision);
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void HandleCollision(GameObject other)
        {
            if (other.CompareTag("Enemy"))
            {
                HealthController enemyHealth = other.GetComponentInParent<HealthController>();
                enemyHealth.TakeDamage(_impactDamage);
            }
            
            Destroy(gameObject);
        }
    }
}
