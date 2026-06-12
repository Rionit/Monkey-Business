using System.Collections;
using MonkeyBusiness.Combat.Health;
using MonkeyBusiness.Combat.Weapons;
using MonkeyBusiness.Managers;
using UnityEngine;
using UnityEngine.Events;

namespace MonkeyBusiness.Combat.Regen
{
    public class SmallAmmoPickup : MonoBehaviour, IAmmoRegen
    {
        /// <summary>
        /// How much ammo to restore
        /// </summary>
        [SerializeField]
        private float _replenishmentPercentage = 20f;
        /// <summary>
        /// How long before this despawns
        /// </summary>
        [SerializeField]
        private float _lifeTime = 10f;
        
        private Coroutine _lifetimeCoroutine;

        public UnityEvent OnCollected { get; private set; } = new UnityEvent();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
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
                var equipManager = other.GetComponentInParent<EquipmentManager>();
                (this as IAmmoRegen).RestoreAmmo(equipManager, _replenishmentPercentage);
                
                StopCoroutine(_lifetimeCoroutine);
                Destroy(gameObject);
            }
        }
    }
}
