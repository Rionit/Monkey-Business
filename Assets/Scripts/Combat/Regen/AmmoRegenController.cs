using System;
using UnityEngine;
using System.Collections;
using Ami.BroAudio;
using MonkeyBusiness.Managers;
using MonkeyBusiness.Misc;
using Sirenix.OdinInspector;
using MonkeyBusiness.Combat.Weapons;
using MonkeyBusiness.Combat.Regen;
using UnityEngine.Events;

namespace MonkeyBusiness.Combat
{
    /// <summary>
    /// Handles rengeneration of ammo for the player when stepping on the ammo regeneration pad.
    /// </summary>
    public class AmmoRegenController : MonoBehaviour, IAmmoRegen
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

        private SoundSource sound;
        public UnityEvent OnCollected { get; private set; } = new UnityEvent();

        bool _canReplenish = true;

        private void Start()
        {
            sound = GetComponent<SoundSource>();
        }

        void OnTriggerEnter(Collider other)
        {
            if(_canReplenish && other.CompareTag("Player"))
            {
                if (sound != null) sound.Play();
                StartCoroutine(ReplenishCoroutine(other.gameObject));
            }
        }
        
        IEnumerator ReplenishCoroutine(GameObject playerObject)
        {
            Debug.Log("Replenishing ammo for player by " + _replenishmentPercentage + "%.");
            _ammoRegenMesh.enabled =false;
            _canReplenish = false;
            var equipManager = playerObject.GetComponentInParent<EquipmentManager>();
            
            (this as IAmmoRegen).RestoreAmmo(equipManager, _replenishmentPercentage);
            
            yield return new WaitForSeconds(_cooldown);

            Debug.Log("Ammo regen pad is ready to use again.");
            _canReplenish = true;
            _ammoRegenMesh.enabled = true;
        }
    }
}
