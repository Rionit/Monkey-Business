
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class HealthController : MonoBehaviour
{
    [field: SerializeField]
    public float MaxHealth { get; private set; } = 100f;

    public float CurrentHealth { get; private set; }

    /// <summary>
    /// Event invoked when health changes, with the new health value as a parameter.
    /// </summary>
    public UnityEvent<float> OnHealthChanged;

    public UnityEvent OnDeath;

    [SerializeField]
    bool godMode = false;

    bool killed = false;

    public void Start()
    {
        CurrentHealth = MaxHealth;
    }


    public void Heal(float amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        OnHealthChanged.Invoke(CurrentHealth);
    }

    public void TakeDamage(float damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0f && !killed && !godMode)
        {
            Die();
        }
        else OnHealthChanged.Invoke(CurrentHealth);
    }

    private void Die()
    {
        killed = true;
        OnDeath.Invoke();
        if(CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        Debug.Log(gameObject.name + " has died.");
        Destroy(gameObject);
    }

}