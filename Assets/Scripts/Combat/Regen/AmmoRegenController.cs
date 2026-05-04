using UnityEngine;
using System.Collections;
using MonkeyBusiness.Managers;
using MonkeyBusiness.Misc;
using Sirenix.OdinInspector;
using MonkeyBusiness.Combat.Weapons;

namespace MonkeyBusiness.Combat
{
    /// <summary>
    /// Handles rengeneration of ammo for the player when stepping on the ammo regeneration pad.
    /// </summary>
    public class AmmoRegenController : MonoBehaviour
    {

        [SerializeField]
        [Tooltip("Percentage of ammo replenished when stepping on the pad.")]
        float _replenishmentPercentage = 20f;

        [SerializeField]
        [Tooltip("Cooldown time in seconds before the pad can be used again.")]
        float _cooldown = 10f;

        [SerializeField]
        [Required]
        [Tooltip("Mesh of the ammo regeneration pad to hide when the pad is on cooldown.")]
        MeshRenderer _ammoRegenMesh;

        [SerializeField]
        [Required]
        [Tooltip("Collider of the ammo regeneration pad to detect player stepping on it.")]
        Collider _collider;

        bool _canReplenish = true;

        void OnTriggerEnter(Collider other)
        {
            if(_canReplenish && other.CompareTag("Player"))
            {
                
                StartCoroutine(ReplenishCoroutine(other.gameObject));
            }
        }
        
        IEnumerator ReplenishCoroutine(GameObject playerObject)
        {
            Debug.Log("Replenishing ammo for player by " + _replenishmentPercentage + "%.");
            _ammoRegenMesh.enabled =false;
            _canReplenish = false;
            var equipManager = playerObject.GetComponentInParent<EquipmentManager>();
            foreach(var item in equipManager.Items)
            {
                if(item is IWeapon weapon)
                {
                    weapon.ReloadPercent(_replenishmentPercentage);
                }
            }

            yield return new WaitForSeconds(_cooldown);

            Debug.Log("Ammo regen pad is ready to use again.");
            _canReplenish = true;
            _ammoRegenMesh.enabled = true;
        }
    }
}
