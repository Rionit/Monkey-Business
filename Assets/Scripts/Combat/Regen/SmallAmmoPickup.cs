using System.Collections;
using MonkeyBusiness.Combat.Health;
using MonkeyBusiness.Combat.Weapons;
using MonkeyBusiness.Managers;
using UnityEngine;

namespace MonkeyBusiness.Combat.Regen
{
    public class SmallAmmoPickup : MonoBehaviour
    {
        [SerializeField]
        private float _replenishmentPercentage = 20f;
        [SerializeField]
        private float _lifeTime = 10f;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            StartCoroutine(StartLifetime(_lifeTime));
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
                var equipManager = other.GetComponentInParent<EquipmentManager>();
                foreach(var item in equipManager.Items)
                {
                    if(item is Weapon weapon)
                    {
                        weapon.ReloadPercent(_replenishmentPercentage);
                    }
                }
                
                Destroy(gameObject);
            }
        }
    }
}
