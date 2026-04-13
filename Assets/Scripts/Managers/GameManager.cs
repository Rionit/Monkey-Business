using UnityEngine;
using Sirenix.OdinInspector;
using MonkeyBusiness.Enemies;
using MonkeyBusiness.Combat;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace MonkeyBusiness.Managers
{
    enum GameState
    {
        PREPARATION,
        COMBAT
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        //private GameState _currentGameState;

        [SerializeField]
        private int _enemiesPerWave = 10;

        private int _enemiesRemaining;

        [SerializeField]
        private int _enemiesSpawnedAtOnce = 2;

        [SerializeField]
        private float _preparationPhaseDuration = 20;

        [SerializeField]
        private float _enemySpawnDelay = 5;

        [SerializeField]
        private GameObject _enemyPrefab;

        [SerializeField]
        private List<Transform> _enemySpawnPoints = new();

        [SerializeField]
        private GameObject _playerCharacter;

        private List<GameObject> _enemies = new();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of GameManager detected! Replacing the old one.");
            }
            Instance = this;
        }

        void Start()
        {
            //_currentGameState = GameState.PREPARATION;

            StartCoroutine(PreparationPhase());

            _enemiesSpawnedAtOnce = Math.Min(_enemiesSpawnedAtOnce, _enemySpawnPoints.Count);
        }

        void SpawnEnemy()
        {
            //Debug.Log($"Spawning {_enemiesSpawnedAtOnce} enemies");
            Debug.Log("Spawning enemy");
        }

        void SpawnDummyEnemy(int spawnPointIndex = 0)
        {
            GameObject enemyObject = Instantiate(_enemyPrefab, _enemySpawnPoints[spawnPointIndex].position, Quaternion.identity);
            
            if(enemyObject.TryGetComponent<EnemyFollowController>(out EnemyFollowController enemyFollowController)){
                enemyFollowController.ChaseTarget = _playerCharacter;
            }else
            {
                Debug.LogError("No EnemyFollowController on enemy prefab");
            }

            if(enemyObject.TryGetComponent<HealthController>(out HealthController healthController))
            {
                healthController.OnDeath.AddListener(OnEnemyDestroyed);
            }
            else
            {
                Debug.LogError("No HealthController on enemy prefab");
            }

            _enemies.Add(enemyObject);
        }

        void OnEnemyDestroyed(GameObject gameObject)
        {
            Debug.Log($"Enemy {gameObject.name} died :D");
            _enemiesRemaining--;

            if (_enemies.Contains(gameObject))
            {
                _enemies.Remove(gameObject);
            }
            else
            {
                Debug.LogWarning("I don't know this enemy D:");
            }

            Debug.Log($"{_enemiesRemaining} enemies remaining");
        }

        private IEnumerator PreparationPhase()
        {
            Debug.Log("Preparation phase started");
            yield return new WaitForSeconds(_preparationPhaseDuration);

            yield return StartCoroutine(CombatPhase());
        }

        private IEnumerator CombatPhase()
        {
            _enemiesRemaining = _enemiesPerWave;
            Debug.Log("Combat phase started");
            while (_enemies.Count < _enemiesRemaining)
            {   
                for(int i = 0; i < _enemiesSpawnedAtOnce; i++)
                {
                    SpawnDummyEnemy(i);
                }
                yield return new WaitForSeconds(_enemySpawnDelay);
            }
            
            yield return new WaitWhile(()=> _enemiesRemaining > 0);

            Debug.Log("All enemies defeated!");
            yield return StartCoroutine(PreparationPhase());
        }
    }
}