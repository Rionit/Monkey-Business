
using Unity;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

class ProjectileController : MonoBehaviour
{
    /// <summary>
    /// Speed of the projectile in units per second.
    /// </summary>
    [SerializeField]
    [Tooltip("Speed of the projectile in units per second")]
    float projectileSpeed = 20f;

    /// <summary>
    /// Damage dealt by the projectile upon hitting an enemy.
    /// </summary>
    [SerializeField]
    [Tooltip("Damage dealt by the projectile")]
    float damage = 10f;

    /// <summary>
    /// Knockback force applied to the enemy upon hit.
    /// </summary>
    [SerializeField]
    [Tooltip("Slowdown applied to the enemy upon hit, in %")]
    [Range(0f, 100f)]
    float hitSlowdown = 5f;

    /// <summary>
    /// Maximum distance the projectile can travel before being destroyed.
    /// </summary>
    [SerializeField]
    [Tooltip("Maximum distance the projectile can travel before being destroyed")]
    float maxDistance = 100f;

    [SerializeField]
    [Tooltip("0 = no post-hit slowdown, >0 = slowdown duration in seconds after hitting an enemy")]
    float postHitSlowdownDuration = 0.0f;

    float distance = 0f;

    string _enemyTag;


    public void Initialize(string enemyTag)
    {
        _enemyTag = enemyTag;
    }

    void Update()
    {
        var deltaDistance = projectileSpeed * Time.deltaTime;
        transform.position +=  deltaDistance * transform.forward;

        distance += deltaDistance;

        if(distance >= maxDistance)
        {
            //Debug.Log("Projectile reached max distance, destroying.");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Colliding .. other tag = " + other.tag + " ... object name = " + other.gameObject.name);
        if(other.tag == _enemyTag)
        {
            var enemyHealth = other.GetComponentInChildren<HealthController>();
            if(enemyHealth == null)
            {
                enemyHealth = other.GetComponentInParent<HealthController>();
            }
            if(enemyHealth == null)
            {
                Debug.LogError("No health controller found on enemy, cannot apply damage.");
                return;
            }

            Debug.Log("Damaging" + other.gameObject.name);
            enemyHealth.TakeDamage(damage);
            if(_enemyTag == "Enemy")
            {
                var speedFactor = 1f - (hitSlowdown / 100f);
                other.GetComponentInChildren<NavMeshAgent>().velocity *= speedFactor;
                if(postHitSlowdownDuration > 0f)
                {
                    object enemyController = other.GetComponentInChildren<MeleeEnemyController>();
                    if(enemyController != null)
                    {
                        ((MonoBehaviour)enemyController).StartCoroutine(((MeleeEnemyController)enemyController).Slowdown(postHitSlowdownDuration, speedFactor));
                    }
                    else
                    {
                        enemyController = other.GetComponentInChildren<RangedEnemyController>();
                        ((MonoBehaviour)enemyController).StartCoroutine(((RangedEnemyController)enemyController).Slowdown(postHitSlowdownDuration, speedFactor));
                    }
                }
            }
        }

        //Debug.Log("Destroying projectile");
        // For now, just destroy the projectile on collision with anything
        if(other.tag != (_enemyTag == "Enemy" ? "Player" : "Enemy"))Destroy(gameObject);

    }
}
