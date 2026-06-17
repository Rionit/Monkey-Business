using MonkeyBusiness.Items;
using MonkeyBusiness.Misc;
using UnityEngine;
using UnityEngine.Serialization;

namespace MonkeyBusiness.Perks
{
    public class CollectiblePerkSpawner : MonoBehaviour
    {
        private GameObject _instance = null;
        
        public bool SpawnItem(GameObject collectiblePerkPrefab)
        {
            if(_instance == null)
            {
                _instance = Instantiate(collectiblePerkPrefab, transform.position, transform.rotation);
                return true;
            }
            return false;
        }
        
        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, Vector3.one * .25f);
            Gizmos.DrawRay(new Ray(transform.position, transform.up));
        }
    }
}
