using System.Collections.Generic;
using Ami.BroAudio;
using MonkeyBusiness.Combat.Health;
using UnityEngine;
using UnityEngine.Serialization;

namespace MonkeyBusiness.Combat.Attack
{
    public class Explosion : MonoBehaviour
    {
        [SerializeField]
        private float _explosionRadius = 10.0f;
        [SerializeField]
        private float _explosionDamage = 400.0f;

        [SerializeField] private float _playerDamageMultiplier = 0.1f;
        
        [SerializeField]
        private bool explodeOnStart = true;

        /// <summary>
        /// What type of entity is hit with this explosion ("Player"/"Enemy")
        /// </summary>
        public string targetEntityType; 
        
        /// <summary>
        /// Particle system for the explosion.
        ///
        /// Make sure the particle system has the property Play on awake enabled and destroys itself when the animation ends.
        /// </summary>
        [SerializeField] private ParticleSystem _explosionParticleSystem;

        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if(explodeOnStart) Explode();
        }

        public void Explode()
        {
            List<Transform> hitEnemies = new();

            // blow tf up
            foreach(Collider collider in Physics.OverlapSphere(transform.position, _explosionRadius, LayerMask.GetMask(targetEntityType, "Default"), QueryTriggerInteraction.Ignore))
            {
                if (!collider.gameObject.CompareTag(targetEntityType))
                {
                    //Debug.LogError("Wrong entity type: " + collider.gameObject.tag + " vs " + targetEntityType);
                    continue;
                }

                if (hitEnemies.Contains(collider.transform.root))
                {
                    //Debug.LogError("Already checked");
                    continue;
                }

                // test obstruction (for some reason player behaved weird af)
                if(targetEntityType != "Player" && Physics.Linecast(transform.position, collider.transform.position, LayerMask.GetMask("Navigation")))
                {
                    //Debug.LogError("Obstructed by navigation layer");
                    // object is obstructed by environment
                    continue;
                }
                
                HealthController healthController = collider.GetComponentInParent<HealthController>();

                if (healthController == null)
                {
                    //Debug.LogError("No health controller found");
                    continue;
                }

                float explosionDamageFactor = Vector3.Distance(transform.position, collider.transform.position) / _explosionRadius;
                explosionDamageFactor = 1 - explosionDamageFactor;
                explosionDamageFactor = Mathf.Min(2 * explosionDamageFactor, 1.0f);
                
                if (targetEntityType == "Player")
                {
                    healthController.TakeDamage(healthController.CurrentHealth * _playerDamageMultiplier * explosionDamageFactor, Vector3.zero);
                }
                else
                {
                    healthController.TakeDamage(_explosionDamage * explosionDamageFactor, (collider.transform.position - transform.position).normalized);
                }
                // prevent damaging the same enemy multiple times by hitting more than one of their hitboxes
                hitEnemies.Add(collider.transform.root);

            }

            _explosionParticleSystem.transform.parent = null;
            _explosionParticleSystem.gameObject.SetActive(true);

            var sound = GetComponent<SoundSource>();
            sound.CurrentPlayer.OnEnd(id => Destroy(gameObject));
            sound.Play();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }
}
