using MonkeyBusiness.Items;
using MonkeyBusiness.Misc;
using UnityEngine;

namespace MonkeyBusiness.Perks
{
    public class MonksterSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject monksterPrefab;
        
        private GameObject _spawnedmonkster = null;
        
        public void SpawnItem()
        {
            if(_spawnedmonkster == null)
            {
                _spawnedmonkster = Instantiate(monksterPrefab, transform.position, transform.rotation);
            }
        }
        
        void OnDrawGizmos()
        {
            Gizmos.color = Color.turquoise;
            Gizmos.DrawWireCube(transform.position, Vector3.one * .25f);
            Gizmos.DrawRay(new Ray(transform.position, transform.up));
        }
    }
}
