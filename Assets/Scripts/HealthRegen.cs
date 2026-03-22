using UnityEngine;
using System.Collections;

public class HealthRegen : MonoBehaviour
{
    [SerializeField]
    float healAmount = 10f;

    [SerializeField]
    float cooldown = 10f;

    [SerializeField]
    MeshRenderer meshRenderer;

    bool canHeal = true;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided with " + other.name);   
        if(canHeal && other.CompareTag("Player"))
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
        meshRenderer.enabled =false;
        canHeal = false;
        healthController.Heal(healAmount);
        yield return new WaitForSeconds(cooldown);
        canHeal = true;
        meshRenderer.enabled = true;
    }
}
