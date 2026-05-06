using System.Collections;
using MonkeyBusiness.Combat.Health;
using UnityEngine;

namespace MonkeyBusiness.Combat.Regen
{
    public class SmallHealthPickup : MonoBehaviour
    {
        /// <summary>
        /// How much to heal on pickup
        /// </summary>
        [SerializeField] 
        private float _healthRestored = 20f;
        /// <summary>
        /// How long before this despawns
        /// </summary>
        [SerializeField]
        private float _lifeTime = 10f;
        private Coroutine _lifetimeCoroutine;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _lifetimeCoroutine = StartCoroutine(StartLifetime(_lifeTime));
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private IEnumerator StartLifetime(float lifetime)
        {
            yield return new WaitForSeconds(lifetime);
            
            Destroy(gameObject);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                HealthController healthController = other.gameObject.GetComponentInParent<HealthController>();
                healthController.Heal(_healthRestored);
                StopCoroutine(_lifetimeCoroutine);
                Destroy(gameObject);
            }
        }
    }
}
