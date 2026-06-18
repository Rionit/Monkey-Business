using System;
using System.Collections;
using System.Collections.Generic;
using MonkeyBusiness.Managers;
using MonkeyBusiness.Misc;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MonkeyBusiness.Perks
{
    public class CollectiblePerksManager : MonoBehaviour
    {
        
        [SerializeField] private GameObject monksterPrefab;
        [SerializeField] private GameObject chimpexPrefab;
        [SerializeField] private GameObject gorillaPrefab;
        [SerializeField] private GameObject crazyapePrefab;
        
        [SerializeField] private float pressureThreshold = 100f;
        
        [SerializeField] private float timeFactor = 0.2f;
        
        [SerializeField] private GameObject spawnersRoot;

        [ShowInInspector, ReadOnly] private float _pressure = 0f;

        private float _timeSinceLastSpawn = 0f;
        
        private CollectiblePerkSpawner[] _spawners;

        private void Start()
        {
            _spawners = spawnersRoot.GetComponentsInChildren<CollectiblePerkSpawner>();
            SpawnCollectiblePerk(monksterPrefab);
            SpawnCollectiblePerk(crazyapePrefab);
            SpawnCollectiblePerk(gorillaPrefab);
            SpawnCollectiblePerk(chimpexPrefab);
            
            GameManager.Instance.OnWaveDefeated.AddListener(OnWaveDefeated);
            
            StartCoroutine(RandomPerkSpawner());
        }

        public void OnWaveDefeated()
        {
            SpawnCollectiblePerk(monksterPrefab);
        }

        [Button("Spawn Monkster")]
        public void SpawnCollectiblePerk(GameObject perkPrefab)
        {
            for (int i = 0; i < 10; i++)
            {
                if (_spawners[Random.Range(0, _spawners.Length)].SpawnItem(perkPrefab))
                    return;
            }
        }

        private void Update()
        {
            _timeSinceLastSpawn += Time.deltaTime;
            
            _pressure = 0f;
            _pressure += GameManager.Instance.AliveEnemies;
            _pressure += _timeSinceLastSpawn * timeFactor;
            _pressure += GameManager.Instance.CurrentWave;

            if (_pressure > pressureThreshold)
            {
                SpawnCollectiblePerk(monksterPrefab);
                _timeSinceLastSpawn = 0f;
            }
        }
        
        private IEnumerator RandomPerkSpawner()
        {
            GameObject[] randomPerks =
            {
                chimpexPrefab,
                gorillaPrefab,
                crazyapePrefab
            };

            while (true)
            {
                yield return new WaitForSeconds(Random.Range(60f, 300f)); // 1-5 minutes

                SpawnCollectiblePerk(randomPerks[Random.Range(0, randomPerks.Length)]);
            }
        }
    }
}
