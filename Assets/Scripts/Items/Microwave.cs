using System;
using System.Collections.Generic;
using MonkeyBusiness.Combat.Health;
using UnityEditor;
using UnityEngine;

namespace MonkeyBusiness.Items
{
    /// <summary>
    /// A throwable object that explodes when thrown
    /// TODO everything
    /// </summary>
    [RequireComponent(typeof(Item))]
    public class Microwave : MonoBehaviour
    {
        [SerializeField]
        private float _explosionRadius = 10.0f;
        [SerializeField]
        private float _explosionDamage = 400.0f;

        /// <summary>
        /// Particle system for the explosion.
        ///
        /// Make sure the particle system has the property Play on awake enabled and destroys itself when the animation ends.
        /// </summary>
        [SerializeField] private ParticleSystem _explosionParticleSystem;

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

        void HandleCollision(GameObject _)
        {
            List<Transform> hitEnemies = new();

            // blow tf up
            foreach(Collider collider in Physics.OverlapSphere(transform.position, _explosionRadius,  LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
            {
                if (!collider.gameObject.CompareTag("Enemy"))
                {
                    continue;
                }

                if (hitEnemies.Contains(collider.transform.root))
                {
                    continue;
                }

                // test obstruction
                if(Physics.Linecast(transform.position, collider.transform.position, LayerMask.GetMask("Navigation")))
                {
                    // object is obstructed by environment
                    continue;
                }
                
                HealthController healthController = collider.GetComponentInParent<HealthController>();

                Debug.Log($"Distance to explostion: {Vector3.Distance(transform.position, collider.transform.position)}");

                float explosionDamageFactor = Vector3.Distance(transform.position, collider.transform.position) / _explosionRadius;
                explosionDamageFactor = 1 - explosionDamageFactor;
                explosionDamageFactor = Mathf.Min(2 * explosionDamageFactor, 1.0f);

                Debug.Log($"Dealing {_explosionDamage * explosionDamageFactor} damage");
                healthController.TakeDamage(_explosionDamage * explosionDamageFactor);
                // prevent damaging the same enemy multiple times by hitting more than one of their hitboxes
                hitEnemies.Add(collider.transform.root);

            }

            _explosionParticleSystem.transform.parent = null;
            _explosionParticleSystem.gameObject.SetActive(true);
            Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }
}
