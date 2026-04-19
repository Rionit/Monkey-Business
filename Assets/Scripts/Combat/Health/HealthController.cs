
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace MonkeyBusiness.Combat.Health
{
    /// <summary>
    /// Manages the entitiy's health, damage taking and death.
    /// </summary>
    public class HealthController : MonoBehaviour
    {
        /// <summary>
        /// Maximum health of the entity.
        /// </summary>
        [field: SerializeField]
        [Tooltip("Maximum health of the entity.")]
        public float MaxHealth { get; private set; } = 100f;

        /// <summary>
        /// Current health of the entity. 
        /// </summary>
        [ShowInInspector]
        [Tooltip("Current health of the entity.")]
        public float CurrentHealth { get; private set; } // BUG: Health value not updated in the inspector

        /// <summary>
        /// Event invoked when health changes, with the new health value as a parameter.
        /// </summary>
        public UnityEvent<float> OnHealthChanged = new();

        /// <summary>
        /// Event invoked when the entity dies. The GameObject of the dying entity is passed as a parameter.
        /// </summary>
        public UnityEvent<GameObject> OnDeath = new();

        /// <summary>
        /// DEBUG: Allows the entity to not die when health reaches zero.
        /// </summary>
        [field: SerializeField]
        [Tooltip("DEBUG: Allows the entity to not die when health reaches zero.")]
        public bool GodMode { get; private set; } = false;

        [BoxGroup("Poison")]
        [SerializeField]
        [Tooltip("Whether the entity has a visual effect when posioned")]
        bool _hasPoisonEffect = false;

        [BoxGroup("Poison")]
        [SerializeField]
        [ShowIf(nameof(_hasPoisonEffect))]
        Renderer _poisonEffectRenderer;

        bool _killed = false;

        Coroutine _poisonCoroutine;

        public void Start()
        {
            CurrentHealth = MaxHealth;
        }

        /// <summary>
        /// Heals the entity the given <paramref name="amount"/>.
        /// </summary>
        public void Heal(float amount)
        {
            CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
            OnHealthChanged.Invoke(CurrentHealth);
        }

        /// <summary>
        /// Damages the entity by the given <paramref name="damage"/> amount.
        /// </summary>
        /// <remarks><i>Invokes <see cref="OnHealthChanged"/> if health is below 0 and god mode is disabled.</i></remarks>
        public void TakeDamage(float damage)
        {
            CurrentHealth -= damage;
            if (CurrentHealth <= 0f && !_killed && !GodMode)
            {
                Die();
            }
            else OnHealthChanged.Invoke(CurrentHealth);

            Debug.Log("Current health " + CurrentHealth);
        }

        public void ApplyPoison(float damagePerTick, float tickInterval, int numTicks)
        {
            if(_poisonCoroutine != null) StopCoroutine(_poisonCoroutine);
            _poisonCoroutine = StartCoroutine(PoisonCoroutine(damagePerTick, tickInterval, numTicks));
        }

        IEnumerator PoisonCoroutine(float damagePerTick, float tickInterval, int numTicks)
        {
            if(_hasPoisonEffect)
            {
                if(_poisonEffectRenderer == null)
                {
                    Debug.LogWarning("Poison effect renderer not assigned for " + gameObject.name);
                }
                else _poisonEffectRenderer.enabled = true;
            }

            Debug.Log("Starting the poison coroutine - Number of health " + CurrentHealth);
            for(int i = 0; i < numTicks; i++)
            {
                TakeDamage(damagePerTick);
                yield return new WaitForSeconds(tickInterval);
            }

            if(_hasPoisonEffect && _poisonEffectRenderer != null)
            {
                _poisonEffectRenderer.enabled = false;
            }

            Debug.Log("Finished the poison coroutine - Number of health " + CurrentHealth);
        }

        private void Die()
        {
            _killed = true; // Prevents this method to be called multiple times
            OnDeath.Invoke(gameObject);

            // TODO: Game Over screen
            if(CompareTag("Player"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            //Debug.Log(gameObject.name + " has died.");
            else Destroy(gameObject);
        }
        
        /// <summary>
        /// Changes the current MaxHealth value to a new <paramref name="amount"/>.
        /// </summary>
        public void SetMaxHealth(float amount)
        {
            // Prevent invalid values
            MaxHealth = Mathf.Max(1f, amount);
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
            OnHealthChanged.Invoke(CurrentHealth);
        }
    }
}