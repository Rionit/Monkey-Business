using UnityEngine;
using MonkeyBusiness.Combat.Health;
using System.Collections.Generic;

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

        /// <summary>
        /// Leave unassigned to not randomize color
        /// </summary>
        [SerializeField]
        private Renderer _mesh;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _item = GetComponentInChildren<Item>();
            _item.OnThrownCollision.AddListener(HandleCollision);
            if (_mesh)
            {
                _mesh.material.color = Random.ColorHSV(0, 1, 0, 1, 0.5f, 1, 1, 1);
            }
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
                enemyHealth.TakeDamage(_impactDamage, (other.transform.position - transform.position).normalized);
            }
            
            Destroy(gameObject);
        }
    }
}
