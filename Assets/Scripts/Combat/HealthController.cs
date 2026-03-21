
using UnityEngine;
using UnityEngine.Events;

public class HealthController : MonoBehaviour
{
    [field: SerializeField]
    public float MaxHealth { get; private set; } = 100f;

    public float CurrentHealth { get; private set; }

    /// <summary>
    /// Event invoked when health changes, with the new health value as a parameter.
    /// </summary>
    public UnityEvent<float> OnHealthChanged;

    public void Start()
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(float damage)
    {
        Debug.Log(gameObject.name + " took " + damage + " damage.");
        CurrentHealth -= damage;
        if (CurrentHealth <= 0f)
        {
            Die();
        }
        else OnHealthChanged.Invoke(CurrentHealth);
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has died.");
        Destroy(gameObject);
    }
}