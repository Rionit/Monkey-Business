using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using MonkeyBusiness.Combat.Health;
using MonkeyBusiness.Enemies.Navigation;
using MonkeyBusiness.Misc;
using System;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using UnityEngine.Rendering.Universal;
using Ami.BroAudio;

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
    /// TODO spawn new items at the start of each round
    /// </summary>
    public class GameManager : MonoBehaviour, ITargetable
    {

        #region Score 
        /// <summary>
        /// Note: Score itself is stored inthe Scoreboard dictionary as a key
        /// </summary>
        public struct ScoreEntry
        {
            public string Name;
            public int Level;
        
            public ScoreEntry(string name, int level)
            {
                Name = name;
                Level = level;
            }
        }

        public static int Score = 0;

        public static int HighScore = 0;

        public static SortedDictionary<int,  List<ScoreEntry>> Scoreboard = new ();

        public static Dictionary<string, int> ScoreboardNamesToScore = new();

        public static int LevelReached = 0;

        const int MAX_SCOREBOARD_ENTRIES = 10;

        public static UnityEvent<int> OnScoreChanged = new();

        const string HIGH_SCORE_KEY = "HighScore";

        const int KILL_SCORE = 100;

        [HideInInspector]
        public float[] multipliers = new float[] { 1.0f, 1.1f, 1.2f, 1.3f, 1.4f, 1.5f };

        float _currentMultiplier = 1f;

        const float SCORE_CHECK_INTERVAL = 0.1f;

        float _timeSinceLastScoreCheck = 0f;

        float _timeSinceLastDamage = 0f;

        float _cumulativeDamage = 0;

        float _damageAddition = 0;

        float _timeUntilFalloff = 5f;

        float _timeForFullFalloff = 5f;

        int _currentMultiplierIndex = 0;

        float[] _multiplierThresholds = new float[] { 1000f, 1500f, 2000f, 2500f, 3000f, 3500f };
        
        /// <summary>
        /// How fast the falloff happens, in %/100 per second. 
        /// </summary>
        Vector2 _falloffRange = new Vector2(0.3f, 1f);

        /// <summary>
        /// Used to notify the UI about the new multiplier and whether it's an increase or decrease (true for increase, false for decrease)
        /// </summary>
        public Action<float, bool> ChangeCumulativeCallback;

        public Action<float, bool> ChangeMultiplerCallback; // float is the multiplier, bool is whether it's an increase (true) or decrease (false)

        #endregion

        #region Spawning
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

        public Action externalOnKillCallback;

        #endregion

        public static GameManager Instance { get; private set; }
        
        //private GameState _currentGameState;
        
        #region Events

        public UnityEvent OnWaveDefeated = new();
        public UnityEvent<int> OnWaveDefeatedNum = new();

        public UnityEvent OnWaveStarted = new();
        public UnityEvent<int> OnEnemyCountChanged = new();

        public UnityEvent<bool> OnPausedOrUnpaused = new();

        #endregion

        [SerializeField] private GameObject _hud;

        [SerializeField]
        GameObject _deathScreen;

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

        

        IInputReceiver[] _inputReceivers;

        Player _playerScript;

        /// <summary>
        /// Currently alive enemies
        /// </summary>
        private List<GameObject> _enemies = new();
        
        public float AliveEnemies => _enemies.Count;

        private InputAction _pauseAction;

        private InputAction _restartAction;

        [SerializeField]
        [Tooltip("Number of enemies to spawn in each wave. \n\n<i>If waves get past the last entry, the last entry will be repeated</i>")]
        List<SpawnInformation> _waveDefinitions = new();

        int _currentWave = 0;
        
        public int CurrentWave => _currentWave;

        Dictionary<GameObject, int> _typesToSpawn = new();

        [SerializeField]
        GameObject _pauseMenu;


        [SerializeField]
        UnityEngine.Rendering.Volume _volume;

        bool _canPause = true;

        public Func<IEnumerator> CountdownCoroutine { set; private get; } 

        [SerializeField]
        private GameObject _itemsRoot;
        
        private ItemSpawner[] _itemSpawners;
        
        /// <summary>
        /// Currently spawned items
        /// </summary>
        private List<GameObject> _items = new();
        
        private bool canSpawnItems = true;
        

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

            _inputReceivers = _playerCharacter.transform.parent.GetComponentsInChildren<IInputReceiver>();
            
            StaticEvents.OnItemRegistered.AddListener(AddItem);
            StaticEvents.OnItemUnregistered.AddListener(RemoveItem);
        }

        void Start()
        {
            //_currentGameState = GameState.PREPARATION;

            Time.timeScale = 1f; // Restarts the time scale
            _restartAction = InputSystem.actions.FindAction("Restart");
            _restartAction.performed += _ => Restart();

            _pauseAction = InputSystem.actions.FindAction("Pause");
            _canPause = true;
            _pauseAction.performed += PauseOrUnpause;
            _playerScript = _playerCharacter.GetComponentInParent<Player>();
            StartCoroutine(PreparationPhase());

            _playerCharacter.GetComponentInParent<HealthController>().OnDeath.AddListener(OnPlayerDeath);
            BroAudio.SetVolume(BroAudioType.All, PlayerPrefs.GetFloat("MasterVolume", 1f));

            _itemSpawners = _itemsRoot.GetComponentsInChildren<ItemSpawner>();
        }

        public void PauseOrUnpause()
        {
            if(!_canPause) return;
            Time.timeScale = Time.timeScale == 0f ? 1f : 0f;
            _pauseMenu.SetActive(Time.timeScale == 0f);
            Cursor.lockState = Time.timeScale == 0f ? CursorLockMode.Confined : CursorLockMode.Locked;

            BlurBackground(Time.timeScale == 0f);

            EnableHUD(Time.timeScale != 0f);

            foreach(var receiver in _inputReceivers)
            {
                receiver.CanReceiveInput = Time.timeScale != 0f;
            }
        }

        void BlurBackground(bool shouldBlur)
        {
            if(_volume.profile.TryGet(out DepthOfField depthOfField))
            {
                depthOfField.focusDistance.value = shouldBlur ? 0f : 10f;
            }
        }

        void EnableHUD(bool enabled)
        {
            _hud.SetActive(enabled);
        }

        public void PauseOrUnpause(InputAction.CallbackContext context)
        {
            if(!_canPause) return;
            
            Time.timeScale = Time.timeScale == 0f ? 1f : 0f;
            bool isPaused = Time.timeScale == 0f;
            
            _pauseMenu.SetActive(isPaused);
            Cursor.lockState = isPaused ? CursorLockMode.Confined : CursorLockMode.Locked;

            BlurBackground(isPaused);
            EnableHUD(!isPaused);

            foreach(var receiver in _inputReceivers)
            {
                receiver.CanReceiveInput = !isPaused;
            }

            OnPausedOrUnpaused?.Invoke(isPaused);
        }
        
        public List<GameObject> GetItems()
        {
            return _items;
        }

        public void SetItems(List<GameObject> items)
        {
            _items = items;
        }

        public void AddItem(GameObject item)
        {
            if (item != null)
            {
                _items.Add(item);
            }
        }

        public void RemoveItem(GameObject item)
        {
            if (item != null)
            {
                _items.Remove(item);
            }
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
                healthController.OnTakenDamage.AddListener(EnemyDamagedCallback);
            }
            else
            {
                Debug.LogError("No HealthController on enemy prefab");
            }

            _enemies.Add(enemyObject);
        }

        void EnemyDamagedCallback(float damage)
        {
            AddScore(Mathf.RoundToInt(damage));
            AddDamage(damage);
        }

        public static void AddKillScore()
        {
            Score += Mathf.RoundToInt(KILL_SCORE * Instance._currentMultiplier);
            OnScoreChanged.Invoke(Score);
        }

        public static void AddScore(int score)
        {
            Score += Mathf.RoundToInt(score * Instance._currentMultiplier);
            OnScoreChanged.Invoke(Score);
        }

        /// <summary>
        /// Callback when an enemy is defeated
        /// </summary>
        /// <param name="gameObject">the defeated enemy</param>
        void OnEnemyDestroyed(GameObject gameObject)
        {

            AddKillScore();

            EnemyDeathController deathController = gameObject.GetComponentInChildren<EnemyDeathController>();
            DeadBodiesManager.Instance.AddDeadBody(deathController);


            externalOnKillCallback?.Invoke();
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
                OnWaveDefeatedNum.Invoke(_currentWave);
                StartCoroutine(PreparationPhase());
            }
            if(_enemiesRemaining < 0)
            {
                Debug.LogWarning("Enemy count below 0, probably more enemies spawned than expected");
            }
        }

        public void StopItemSpawnThisWave()
        {
            canSpawnItems = false;
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
            foreach(var receiver in _inputReceivers)
            {
                receiver.CanReceiveInput = false;
            }
            Cursor.lockState = CursorLockMode.Confined;
            _canPause = false;
            yield return new WaitUntil(() => _perkSelected);
            Cursor.lockState = CursorLockMode.Locked;
            _hud.SetActive(true);
            _perkSelected = false;
            _canPause = true;

            foreach(var receiver in _inputReceivers)
            {
                receiver.CanReceiveInput = true;
            }

            //Debug.Log("Preparation phase started");
            //yield return new WaitForSeconds(_preparationPhaseDuration);
            
            StartCoroutine(CombatPhase());
        }

        /// <summary>
        /// Combat phase coroutine
        /// </summary>
        /// <returns></returns>
        private IEnumerator CombatPhase()
        {
            yield return new WaitForSeconds(2f);
            if (CountdownCoroutine != null)
                yield return CountdownCoroutine(); // Waits for countdown coroutine

            OnWaveStarted?.Invoke();
            var waveInfo = _waveDefinitions[Mathf.Min(_currentWave, _waveDefinitions.Count - 1)];
            _typesToSpawn = new();
            _typesToSpawn[gorillaPrefab] = waveInfo.gorillas;
            _typesToSpawn[chimpPrefab] = waveInfo.chimps;

            _enemiesRemaining = waveInfo.gorillas + waveInfo.chimps;
            OnEnemyCountChanged.Invoke(_enemiesRemaining);

            if (canSpawnItems)
            {
                // Make the player drop his held item at the end of the wave in the GUI so we don't destroy something the EquipmentManager has a reference to. Very icky, no good.
                foreach(ItemSpawner itemSpawner in _itemSpawners)
                {
                    itemSpawner.SpawnItem();
                }
            }
            canSpawnItems = true; // reset back

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

            _canPause = false; // Can't pause when dead, obviously
            LevelReached = _currentWave;
            foreach(var receiver in _inputReceivers)
            {
                receiver.CanReceiveInput = false;
            }
            
            Cursor.lockState = CursorLockMode.Confined;
        }

        void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        void OnDestroy()
        {
            SaveScoreboard();

            StaticEvents.ClearAllEvents();
            _pauseAction.performed -= PauseOrUnpause;
        }

        public static SortedDictionary<int, List<string>> SetupScoreboard()
        {
            SortedDictionary<int, List<string>> scoreboard = new SortedDictionary<int,List<string>>();
            for(int i = 0; i < MAX_SCOREBOARD_ENTRIES; i++)
            {
                int score = PlayerPrefs.GetInt($"Scoreboard_{i}_Score", int.MinValue);
                string name = PlayerPrefs.GetString($"Scoreboard_{i}_Name");
                if(score > 0)
                {
                    ScoreboardNamesToScore[name] = score;
                    if(!scoreboard.ContainsKey(score))
                    {
                        scoreboard[score] = new List<string>();
                    }
                    scoreboard[score].Add(name);
                }
            }
            return scoreboard;
        }

        static void SaveScoreboard()
        {
            int index = 0;
            foreach(var entry in Scoreboard)
            {
                if(entry.Key <= 0) continue; // Don't save non-positive scores
                foreach(var data in entry.Value)
                {
                    if(index >= MAX_SCOREBOARD_ENTRIES) return; // Only save top entries
                    PlayerPrefs.SetInt($"Scoreboard_{index}_Score", entry.Key);
                    PlayerPrefs.SetString($"Scoreboard_{index}_Name", data.Name);
                    PlayerPrefs.SetInt($"Scoreboard_{index}_Level", data.Level);

                    index++;
                }
            }
        }


        void AddDamage(float damage)
        {
            Debug.Log("Adding " + damage + " to damage addition");
            _damageAddition += damage;
        }

        void Update()
        {
            var dt = Time.deltaTime;
            _timeSinceLastDamage += dt;
            _timeSinceLastScoreCheck += dt;

            // Cumulative damage increase
            if(_timeSinceLastScoreCheck >= SCORE_CHECK_INTERVAL)
            {
                if(_damageAddition > 0)
                {
                    _cumulativeDamage += _damageAddition;
                    _damageAddition = 0;
                    _timeSinceLastDamage = 0f;

                    Debug.Log("Damage added to cumulative damage, now " + _cumulativeDamage);
                
                    // Check for multiplier increase
                    if(_cumulativeDamage >= _multiplierThresholds[_currentMultiplierIndex])
                    {
                        bool increased = _currentMultiplierIndex < _multiplierThresholds.Length - 1;
                        // We can increase the modifier
                        if(increased)
                        {
                            _cumulativeDamage -= _multiplierThresholds[_currentMultiplierIndex];

                            _currentMultiplierIndex++;
                            _currentMultiplier = multipliers[_currentMultiplierIndex];
                        
                            ChangeMultiplerCallback(_currentMultiplier, true);
                        }
                        else // We cannot increase the modifier (we're already at max modifier)
                        {
                            _cumulativeDamage = _multiplierThresholds[_currentMultiplierIndex] - 1; // Just cap it at the max multiplier threshold
                        }

                        Debug.Log("Changing cumulative to " + (_cumulativeDamage / _multiplierThresholds[_currentMultiplierIndex]) + " for multiplier index " + _currentMultiplierIndex);
                    }
                    ChangeCumulativeCallback(_cumulativeDamage / _multiplierThresholds[_currentMultiplierIndex], true);
                }
                _timeSinceLastScoreCheck = 0f;
            }

            // Damage falloff
            else if(_timeSinceLastDamage >= _timeUntilFalloff)
            {
                // Only happens when the multiplier is not at the base level
                if(_currentMultiplierIndex > 0 || _cumulativeDamage > 0)
                {
                    Debug.Log("Falloff happening");
                    float falloffMultiplier = Mathf.Lerp(_falloffRange.x, _falloffRange.y, (_timeSinceLastDamage - _timeUntilFalloff) / _timeForFullFalloff);
                    float falloffValue = falloffMultiplier * _multiplierThresholds[_currentMultiplierIndex] * dt;
                
                    _cumulativeDamage -= falloffValue;

                    // Underflow to the previous multiplier
                    if(_cumulativeDamage < 0)
                    {
                        bool decreases = _currentMultiplierIndex > 0;
                        if(decreases)
                        {
                            _currentMultiplierIndex--;
                            _currentMultiplier = multipliers[_currentMultiplierIndex];
                            ChangeMultiplerCallback(_currentMultiplier, false);

                            _cumulativeDamage = _multiplierThresholds[_currentMultiplierIndex];
                        }
                        else
                        {
                            _cumulativeDamage = 0;
                        }
                    }


                    ChangeCumulativeCallback(_cumulativeDamage / _multiplierThresholds[_currentMultiplierIndex], false);
                }
            }
        }
    }
}