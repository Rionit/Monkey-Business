using System.Collections.Generic;
using UnityEngine;

namespace MonkeyBusiness.Misc
{
    /// <summary>
    /// Class that implements spawning loot into the game world
    /// </summary>
    public class Dropper : MonoBehaviour
    {
        /// <summary>
        /// List of all droppable loot.
        /// </summary>
        [SerializeField] private List<GameObject> _lootPool;
        
        /// <summary>
        /// Chance to drop an item
        /// </summary>
        [SerializeField] private float _dropChance = 0.67f;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// Drop a random item from loot table if RNG check succeeds
        /// </summary>
        /// <param name="guaranteed"> Ignore drop chance and always drop something if true </param>
        public void DropRandomFromPool(bool guaranteed = false)
        {
            if (guaranteed || Random.value < _dropChance)
            {
                // Drop random item
                Instantiate(_lootPool[Random.Range(0, _lootPool.Count)], transform.position, Quaternion.identity );
            }
        }
    }
}
