using System.Collections;
using Ami.BroAudio;
using MonkeyBusiness.Combat.Health;
using UnityEngine;
using UnityEngine.Events;

namespace MonkeyBusiness.Combat.Regen
{
    public class SmallHealthPickup : MonoBehaviour, IHealthRegen
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

        public UnityEvent OnCollected { get; private set; } = new UnityEvent();

        private SoundSource sound;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            sound = GetComponent<SoundSource>();
            _lifetimeCoroutine = StartCoroutine(StartLifetime(_lifeTime));
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
                if(sound != null) sound.Play();
                HealthController healthController = other.gameObject.GetComponentInParent<HealthController>();
                //healthController.Heal(_healthRestored);
                (this as IHealthRegen).RestoreHealth(healthController, _healthRestored);
                StopCoroutine(_lifetimeCoroutine);
                Destroy(gameObject);
            }
        }
    }
}
