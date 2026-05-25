using System.Collections.Generic;
using UnityEngine;

namespace MonkeyBusiness.Misc
{
    public class ItemSpawner : MonoBehaviour
    {

        [SerializeField]
        private List<GameObject> _spawnableItems;

        private GameObject _spawnedItem = null;

        [SerializeField]
        private float _spawnChance = .33f;


        public void SpawnItem()
        {
            ClearItem();

            if(Random.value < _spawnChance)
            {
                // Drop random item
                _spawnedItem = Instantiate(_spawnableItems[Random.Range(0, _spawnableItems.Count)], transform.position, transform.rotation);
            }
        }

        /// <summary>
        /// Spawns a specific item. Can be used to override the default spawned item
        /// </summary>
        /// <param name=""></param>
        /// <param name=""></param>
        public void SpawnItem(GameObject obj)
        {
            ClearItem();

            if(Random.value < _spawnChance)
            {
                _spawnedItem = Instantiate(obj, transform.position, transform.rotation);
            }
        }


        public void ClearItem()
        {
            if (!_spawnedItem)
            {
                return;
            }

            Destroy(_spawnedItem);
            _spawnedItem = null;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.limeGreen;
            Gizmos.DrawWireCube(transform.position, Vector3.one * .25f);
            Gizmos.DrawRay(new Ray(transform.position, transform.forward));
        }
    }
}
