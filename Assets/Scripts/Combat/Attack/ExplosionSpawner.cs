using MonkeyBusiness.Combat.Attack;
using MonkeyBusiness.Managers;
using UnityEngine;

namespace MonkeyBusiness.Combat.Weapons
{
    public class ExplosionSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private bool isChimpexNeeded = false;
        
        public void SpawnExplosion()
        {
            if (isChimpexNeeded && !StatsManager.Instance.IsChimpexActive) return;
            
            GameObject explosion = GameObject.Instantiate(explosionPrefab, transform);
            explosion.GetComponent<Explosion>().targetEntityType = "Enemy";
        }
    }
}
