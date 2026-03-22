using UnityEngine;

public class SpawnerController : MonoBehaviour
{
    [SerializeField]
    GameObject[] prefabs;

    [SerializeField]
    [Tooltip("True = spawning on cooldown, False = spawning new enemy after previous one dies")]
    bool cooldownBasedSpawning = false;

    [SerializeField]
    float spawnCooldown = 5f;

    [SerializeField]
    int enemiesToSpawn = 5;

    [SerializeField]
    int randomSeed = 12345;

    float _currentTime = 0f;

    System.Random _random;

    [SerializeField]
    Transform playerTransform;

    [SerializeField]
    GameObject projectileParent;
    
    void Start()
    {
        _random = new System.Random(randomSeed);
        if(!cooldownBasedSpawning)
        {
            SpawnRandomEnemy();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(cooldownBasedSpawning)
        {
            _currentTime += Time.deltaTime;

            if(enemiesToSpawn > 0 && _currentTime >= spawnCooldown)
            {
                SpawnRandomEnemy();
                _currentTime = 0f;
            }
        }
    }
    void SpawnRandomEnemy()
    {
        Debug.Log("Spawning");
        int randomIndex = _random.Next(prefabs.Length);
        GameObject prefabToSpawn = prefabs[randomIndex];
        var instance = Instantiate(prefabToSpawn, transform.position, Quaternion.identity);

        var enemyController = instance.GetComponent<IEnemyController>();
        enemyController.Initialize(playerTransform);

        var rangedEnemyController = instance.GetComponent<RangedEnemyController>();
        if(rangedEnemyController != null)
        {
            rangedEnemyController.projectileParent = projectileParent;
        }

        enemiesToSpawn--;
        if(!cooldownBasedSpawning)
        {
            // If not cooldown based, subscribe to the enemy's death event to spawn a new one
            HealthController healthController = instance.GetComponentInChildren<HealthController>();
            if (healthController != null && enemiesToSpawn > 0)
            {
                healthController.OnDeath.AddListener(SpawnRandomEnemy);
            }
        }
    }
}
