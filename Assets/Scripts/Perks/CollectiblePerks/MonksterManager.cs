using System;
using System.Collections.Generic;
using MonkeyBusiness.Managers;
using MonkeyBusiness.Misc;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MonkeyBusiness.Perks
{
    public class MonksterManager : MonoBehaviour
    {
        
        [SerializeField] private float pressureThreshold = 100f;
        
        [SerializeField] private float timeFactor = 0.2f;
        
        [SerializeField] private GameObject spawnersRoot;

        [ShowInInspector, ReadOnly] private float _pressure = 0f;

        private float _timeSinceLastSpawn = 0f;
        
        private MonksterSpawner[] _spawners;

        private void Start()
        {
            _spawners = spawnersRoot.GetComponentsInChildren<MonksterSpawner>();
            _spawners[Random.Range(0, _spawners.Length)].SpawnItem();
        }

        [Button("Spawn Monkster")]
        public void SpawnMonkster()
        {
            _spawners[Random.Range(0, _spawners.Length)].SpawnItem();
        }

        private void Update()
        {
            _timeSinceLastSpawn += Time.deltaTime;
            
            _pressure = 0f;
            _pressure += GameManager.Instance.AliveEnemies;
            _pressure += _timeSinceLastSpawn * timeFactor * GameManager.Instance.CurrentWave;

            if (_pressure > pressureThreshold)
            {
                _spawners[Random.Range(0, _spawners.Length)].SpawnItem();
                _timeSinceLastSpawn = 0f;
            }
        }
    }
}
