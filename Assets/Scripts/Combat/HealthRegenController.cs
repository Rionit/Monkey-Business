using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections;

namespace MonkeyBusiness.Combat
{
    /// <summary>
    /// Restores some health to the player after stepping on the regeneration pad.
    /// </summary>
    public class HealthRegenController : MonoBehaviour
    {
        [SerializeField]
        [Required]
        Collider _collider;

        [SerializeField]
        [Tooltip("Amount of health restored when stepping on the pad.")]
        [Range(0f,100f)]
        float _healthRestored = 20;

        [SerializeField]
        [Tooltip("Cooldown time in seconds before the pad can be used again.")]
        float _cooldown = 10f;

        [SerializeField]
        [Required]
        [Tooltip("Mesh of the heal to hide when the pad is on cooldown.")]
        MeshRenderer _healMesh;

        bool _canHeal = true;

        void OnTriggerEnter(Collider other)
        {
            Debug.Log("Collided with " + other.name);   
            if(_canHeal && other.CompareTag("Player"))
            {
                var healthController = other.GetComponentInParent<HealthController>();
                if(healthController != null)
                {
                    StartCoroutine(HealCoroutine(healthController));
                }
            }
        }

        IEnumerator HealCoroutine(HealthController healthController)
        {
            Debug.Log("Healing player for " + _healthRestored + " health.");
            _healMesh.enabled =false;
            _canHeal = false;
            healthController.Heal(_healthRestored);
            yield return new WaitForSeconds(_cooldown);

            Debug.Log("Health regen pad is ready to use again.");
            _canHeal = true;
            _healMesh.enabled = true;
        }
    }
}
