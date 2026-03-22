using UnityEngine;
using System.Collections;

public class AmmoRegen : MonoBehaviour
{
    [SerializeField]
    float replenishmentPercentage = 20f;

    [SerializeField]
    float cooldown = 10f;

    [SerializeField]
    MeshRenderer meshRenderer;

    bool canReplenish = true;

    void OnTriggerEnter(Collider other)
    {
        if(canReplenish && other.CompareTag("Player"))
        {
            var playerController = other.GetComponentInParent<Player>();
            if(playerController != null)
            {
                StartCoroutine(ReplenishCoroutine(playerController));
            }
        }
    }
    
    IEnumerator ReplenishCoroutine(Player playerController)
    {
        meshRenderer.enabled =false;
        canReplenish = false;
        playerController.ReloadWeapons(replenishmentPercentage);
        yield return new WaitForSeconds(cooldown);
        canReplenish = true;
        meshRenderer.enabled = true;
    }
}
