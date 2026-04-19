using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using MonkeyBusiness.Combat.Health;
using MonkeyBusiness.Enemies.Navigation;
using System;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace MonkeyBusiness.Managers
{
    enum GameState
    {
        PREPARATION,
        COMBAT
    }

    /// <summary>
    /// Manages the game and the game phases
    /// 
    /// 
    /// TODO spawn new items at the start of each round
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        //private GameState _currentGameState;
        
        public UnityEvent OnWaveDefeated = new();
        public UnityEvent<int> OnEnemyCountChanged = new();

        [SerializeField] private GameObject _hud;

        private bool _perkSelected = true;
        
        /// <summary>
        /// How many enemies in total are spawned per wave
        /// </summary>
        [SerializeField]
        private int _enemiesPerWave = 10;

        /// <summary>
        /// How many enemies remain until the wave ends
        /// </summary>
        private int _enemiesRemaining;

        /// <summary>
        /// How many enemies are spawned at once
        /// </summary>
        [SerializeField]
        private int _enemiesSpawnedAtOnce = 2;

        /// <summary>
        /// How long the preparation phase lasts in seconds
        /// </summary>
        [SerializeField]
        private float _preparationPhaseDuration = 20;

        /// <summary>
        /// Delay between individual enemy spawns in seconds
        /// </summary>
        [SerializeField]
        private float _enemySpawnDelay = 5;

        /// <summary>
        /// Prefab of the enemy to spawn
        /// </summary>
        [SerializeField]
        private List<GameObject> _enemyPrefabs;

        /// <summary>
        /// List of all enemy spawn points
        /// </summary>
        [SerializeField]
        private List<Transform> _enemySpawnPoints = new();

        /// <summary>
        /// The player's character object, used for enemy targeting
        /// </summary>
        [SerializeField]
        private GameObject _playerCharacter;

        /// <summary>
        /// Currently alive enemies
        /// </summary>
        private List<GameObject> _enemies = new();
        
        private InputAction _restartAction;

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

            _restartAction = InputSystem.actions.FindAction("Restart");
            _restartAction.performed += _ => Restart();

            StartCoroutine(PreparationPhase());

            _enemiesSpawnedAtOnce = Math.Min(_enemiesSpawnedAtOnce, _enemySpawnPoints.Count);
        }

        void SpawnEnemy()
        {
            //Debug.Log($"Spawning {_enemiesSpawnedAtOnce} enemies");
            Debug.Log("Spawning enemy");
        }

        /// <summary>
        /// Spawns the testing enemy
        /// </summary>
        /// <param name="spawnPointIndex">Index of the spawn point</param>
        void SpawnEnemy(int spawnPointIndex = 0)
        {
            GameObject enemyObject = Instantiate(_enemyPrefabs[Random.Range(0, _enemyPrefabs.Count)], _enemySpawnPoints[spawnPointIndex].position, Quaternion.identity);
            
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

        /// <summary>
        /// Callback when an enemy is defeated
        /// </summary>
        /// <param name="gameObject">the defeated enemy</param>
        void OnEnemyDestroyed(GameObject gameObject)
        {
            Debug.Log($"Enemy {gameObject.name} died :D");
            _enemiesRemaining--;
            OnEnemyCountChanged.Invoke(_enemiesRemaining);

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

        public void PerkSelected()
        {
            _perkSelected = true;
        }

        public GameObject GetPlayerCharacter()
        {
            return _playerCharacter;
        }
        
        /// <summary>
        /// Preparation phase coroutine
        /// </summary>
        /// <returns></returns>
        private IEnumerator PreparationPhase()
        {
            Debug.Log("Perk selection started");
            _hud.SetActive(false);
            Cursor.lockState = CursorLockMode.Confined;
            yield return new WaitUntil(() => _perkSelected);
            Cursor.lockState = CursorLockMode.Locked;
            _hud.SetActive(true);
            _perkSelected = false;
            
            Debug.Log("Preparation phase started");
            yield return new WaitForSeconds(_preparationPhaseDuration);
            
            yield return StartCoroutine(CombatPhase());
        }

        /// <summary>
        /// Combat phase coroutine
        /// </summary>
        /// <returns></returns>
        private IEnumerator CombatPhase()
        {
            _enemiesRemaining = _enemiesPerWave;
            OnEnemyCountChanged.Invoke(_enemiesRemaining);
            Debug.Log("Combat phase started");
            while (_enemies.Count < _enemiesRemaining)
            {   
                for(int i = 0; i < _enemiesSpawnedAtOnce; i++)
                {
                    SpawnEnemy(i);
                }
                yield return new WaitForSeconds(_enemySpawnDelay);
            }
            
            yield return new WaitWhile(()=> _enemiesRemaining > 0);

            Debug.Log("All enemies defeated!");
            OnWaveDefeated.Invoke();
            yield return StartCoroutine(PreparationPhase());
        }

        void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}