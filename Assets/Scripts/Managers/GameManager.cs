using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using MonkeyBusiness.Combat.Health;
using MonkeyBusiness.Enemies.Navigation;
using MonkeyBusiness.Misc;
using System;
using Ami.BroAudio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using TMPro;

namespace MonkeyBusiness.Managers
{
    using Player = Player.Player;

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
    public class GameManager : MonoBehaviour, ITargetable
    {
        public static int Score = 0;

        public static int HighScore = 0;

        public static UnityEvent<int> OnScoreChanged = new();

        const string HIGH_SCORE_KEY = "HighScore";

        const int KILL_SCORE = 100;

        [Serializable]
        class SpawnInformation
        {
            /// <summary>
            /// How many gorillas to spawn in this wave.
            /// </summary>
            public int gorillas;

            /// <summary>
            /// How many chimps to spawn in this wave.
            /// </summary>
            public int chimps;

            /// <summary>
            /// How many enemies to spawn at once in this wave.
            /// </summary>
            public int enemiesPerSpawn;

            /// <summary>
            /// How many enemies there can be at once.
            /// </summary>
            public int enemiesAtOnce; 
        }

        public static GameManager Instance { get; private set; }
        
        //private GameState _currentGameState;
        
        public UnityEvent OnWaveDefeated = new();
        public UnityEvent OnWaveStarted = new();
        public UnityEvent<int> OnEnemyCountChanged = new();

        [SerializeField] private GameObject _hud;

        [SerializeField]
        GameObject _deathScreen;


        [SerializeField]
        TMP_Text _scoreText;

        [SerializeField]
        [RequiredIn(PrefabKind.InstanceInScene)]
        EquipmentManager _equipmentManager;

        private bool _perkSelected = true;

        /// <summary>
        /// How many enemies remain until the wave ends
        /// </summary>
        private int _enemiesRemaining;

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

        [SerializeField]
        [Tooltip("Prefab for the gorilla enemy")]
        GameObject gorillaPrefab;

        [SerializeField]
        [Tooltip("Prefab for the chimp enemy")]
        GameObject chimpPrefab;

        [SerializeField]
        [Obsolete("Deprecated, maintained to work with old game manager, will be removed in the future. Use gorillaPrefab and chimpPrefab instead")]
        private List<GameObject> _enemyPrefabs;

        /// <summary>
        /// Returns the player character as the target.
        /// </summary>
        public GameObject Target => _playerCharacter;

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
        /// Player's character object, used for enemy targeting
        /// </summary>
        public GameObject PlayerCharacter => _playerCharacter;


        Player _playerScript;

        /// <summary>
        /// Currently alive enemies
        /// </summary>
        private List<GameObject> _enemies = new();

        private InputAction _pauseAction;

        private InputAction _restartAction;

        [SerializeField]
        [Tooltip("Number of enemies to spawn in each wave. \n\n<i>If waves get past the last entry, the last entry will be repeated</i>")]
        List<SpawnInformation> _waveDefinitions = new();

        int _currentWave = 0;

        Dictionary<GameObject, int> _typesToSpawn = new();

        [SerializeField]
        GameObject _pauseMenu;

        bool _canPause = true;


        [SerializeField]
        private GameObject _itemsRoot;
        
        private ItemSpawner[] _itemSpawners;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of GameManager detected! Replacing the old one.");
            }
            Instance = this;
            _canPause = true;
            Score = 0;
        }

        void Start()
        {
            //_currentGameState = GameState.PREPARATION;

            Time.timeScale = 1f; // Restarts the time scale
            _restartAction = InputSystem.actions.FindAction("Restart");
            _restartAction.performed += _ => Restart();

            _pauseAction = InputSystem.actions.FindAction("Pause");
            _pauseAction.performed += PauseOrUnpause;
            _playerScript = _playerCharacter.GetComponentInParent<Player>();
            StartCoroutine(PreparationPhase());

            _playerCharacter.GetComponentInParent<HealthController>().OnDeath.AddListener(OnPlayerDeath);
            _scoreText.text = Score.ToString();
            BroAudio.SetVolume(BroAudioType.All, PlayerPrefs.GetFloat("MasterVolume", 1f));

            _itemSpawners = _itemsRoot.GetComponentsInChildren<ItemSpawner>();
        }

        public void PauseOrUnpause()
        {
            if(!_canPause) return;
            Time.timeScale = Time.timeScale == 0f ? 1f : 0f;
            _pauseMenu.SetActive(Time.timeScale == 0f);
            Cursor.lockState = Time.timeScale == 0f ? CursorLockMode.Confined : CursorLockMode.Locked;

            _equipmentManager.CanReceiveInput = Time.timeScale != 0f;
            _playerScript.CanReceiveInput = Time.timeScale != 0f;
        }

        public void PauseOrUnpause(InputAction.CallbackContext context)
        {
            if(!_canPause) return;
            Time.timeScale = Time.timeScale == 0f ? 1f : 0f;
            _pauseMenu.SetActive(Time.timeScale == 0f);
            Cursor.lockState = Time.timeScale == 0f ? CursorLockMode.Confined : CursorLockMode.Locked;

            _equipmentManager.CanReceiveInput = Time.timeScale != 0f;
            _playerScript.CanReceiveInput = Time.timeScale != 0f;
        }

        /// <summary>
        /// Spawns the testing enemy
        /// </summary>
        /// <param name="spawnPointIndex">Index of the spawn point</param>
        void SpawnEnemy(GameObject enemy, int spawnPointIndex = 0)
        {
            GameObject enemyObject = Instantiate(enemy, _enemySpawnPoints[spawnPointIndex].position, Quaternion.identity);
            
            if(enemyObject.TryGetComponent<EnemyFollowController>(out EnemyFollowController enemyFollowController))
            {
                enemyFollowController.ChaseObject = _playerCharacter.GetComponent<ITargetable>().Target;
            }
            else
            {
                Debug.LogError("No EnemyFollowController on enemy prefab");
            }

            if(enemyObject.TryGetComponent<HealthController>(out HealthController healthController))
            {
                healthController.OnDeath.AddListener(OnEnemyDestroyed);
                healthController.OnTakenDamage.AddListener(damage =>
                {
                    AddScore(Mathf.RoundToInt(damage));
                    _scoreText.text = Score.ToString();
                });
            }
            else
            {
                Debug.LogError("No HealthController on enemy prefab");
            }

            _enemies.Add(enemyObject);
        }

        public static void AddKillScore()
        {
            Score += KILL_SCORE;

        }

        public static void AddScore(int score)
        {
            Score += score;
        }



        /// <summary>
        /// Callback when an enemy is defeated
        /// </summary>
        /// <param name="gameObject">the defeated enemy</param>
        void OnEnemyDestroyed(GameObject gameObject)
        {
            AddKillScore();
            _scoreText.text = Score.ToString();
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

            if(_enemiesRemaining == 0)
            {
                Debug.Log("Wave defeated!");
                _currentWave++;
                OnWaveDefeated.Invoke();
                StartCoroutine(PreparationPhase());
            }
            if(_enemiesRemaining < 0)
            {
                Debug.LogWarning("Enemy count below 0, probably more enemies spawned than expected");
            }
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
            _playerScript.CanReceiveInput = false;
            _equipmentManager.CanReceiveInput = false;

            Cursor.lockState = CursorLockMode.Confined;
            yield return new WaitUntil(() => _perkSelected);
            Cursor.lockState = CursorLockMode.Locked;
            _hud.SetActive(true);
            _perkSelected = false;

            _playerScript.CanReceiveInput = true;
            _equipmentManager.CanReceiveInput = true;
            
            Debug.Log("Preparation phase started");
            yield return new WaitForSeconds(_preparationPhaseDuration);
            
            StartCoroutine(CombatPhase());
        }

        /// <summary>
        /// Combat phase coroutine
        /// </summary>
        /// <returns></returns>
        private IEnumerator CombatPhase()
        {
            OnWaveStarted?.Invoke();
            var waveInfo = _waveDefinitions[Mathf.Min(_currentWave, _waveDefinitions.Count - 1)];
            _typesToSpawn = new();
            _typesToSpawn[gorillaPrefab] = waveInfo.gorillas;
            _typesToSpawn[chimpPrefab] = waveInfo.chimps;

            _enemiesRemaining = waveInfo.gorillas + waveInfo.chimps;
            OnEnemyCountChanged.Invoke(_enemiesRemaining);

            // Make the player drop his held item at the end of the wave in the GUI so we don't destroy something the EquipmentManager has a reference to. Very icky, no good.
            foreach(ItemSpawner itemSpawner in _itemSpawners)
            {
                itemSpawner.SpawnItem();
            }

            Debug.Log("Combat phase started");
            while (_enemies.Count < _enemiesRemaining)
            {   
                int possibleEnemies =  Mathf.Min(waveInfo.enemiesPerSpawn, Mathf.Min(waveInfo.enemiesAtOnce - _enemies.Count, _enemiesRemaining - _enemies.Count)); 

                Debug.Assert(_enemiesRemaining >= _enemies.Count, "Enemies remaining should never be less than currently alive enemies");

                int spawnableEnemies= Mathf.Max(_enemiesRemaining - _enemies.Count, 0);

                int toSpawn = Mathf.Min(possibleEnemies, spawnableEnemies);

                if(_enemies.Count + toSpawn > _enemiesRemaining)
                {
                    Debug.LogError("Trying to spawn more enemies than remaining! This should not happen, check the spawn logic!");
                }
            
                Debug.Log("Spawning " + Mathf.Min(possibleEnemies, spawnableEnemies) + " enemies. " + _enemies.Count + "/" + _enemiesRemaining + " currently alive.");

                for(int i = 0; i < Mathf.Min(possibleEnemies, spawnableEnemies); i++)
                {
                    int totalToSpawn = _typesToSpawn[gorillaPrefab] + _typesToSpawn[chimpPrefab];
                    int randomPick = Random.Range(0, totalToSpawn); // Picks random type to pick

                    if(randomPick < _typesToSpawn[gorillaPrefab]) // Spawn gorilla
                    {
                        SpawnEnemy(gorillaPrefab, i % _enemySpawnPoints.Count);
                        _typesToSpawn[gorillaPrefab]--;
                    }
                    else // Spawn chimp
                    {
                        SpawnEnemy(chimpPrefab, i % _enemySpawnPoints.Count);
                        _typesToSpawn[chimpPrefab]--;
                    }

                    // Small wait to avoid collisions
                    yield return new WaitForSeconds(0.2f);
                }

                yield return new WaitForSeconds(_enemySpawnDelay);
            }
        }

        void OnPlayerDeath(GameObject _)
        {
            Time.timeScale = 0f; // Freezes the game
            _hud.SetActive(false);
            _deathScreen.SetActive(true);
            _equipmentManager.CanReceiveInput = false;
            _playerScript.CanReceiveInput = false;

            Cursor.lockState = CursorLockMode.Confined;
        }



        void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        void OnDestroy()
        {
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, HighScore);
        }
    }
}