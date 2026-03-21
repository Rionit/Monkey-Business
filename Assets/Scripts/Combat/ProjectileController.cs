
using Unity;
using UnityEngine;
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
    [Tooltip("Knockback force applied to the enemy upon hit")]
    float knockbackForce = 5f;

    /// <summary>
    /// Maximum distance the projectile can travel before being destroyed.
    /// </summary>
    [SerializeField]
    [Tooltip("Maximum distance the projectile can travel before being destroyed")]
    float maxDistance = 100f;

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
            Debug.Log("Projectile reached max distance, destroying.");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Colliding");
        if(other.tag == _enemyTag)
        {
            var enemyHealth = other.GetComponentInChildren<HealthController>();
            enemyHealth.TakeDamage(damage);
            other.attachedRigidbody.AddExplosionForce(knockbackForce , transform.position, 1f, 0f, ForceMode.Impulse);
        }

        Debug.Log("Destroying projectile");
        // For now, just destroy the projectile on collision with anything
        Destroy(gameObject);

    }
}
