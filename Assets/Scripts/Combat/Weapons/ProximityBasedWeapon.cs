using UnityEngine;
using MonkeyBusiness.Misc;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine.UI;

namespace MonkeyBusiness.Combat.Weapons
{
    public abstract class ProximityBasedWeapon : MonoBehaviour, IWeapon
    {
        public bool HasAmmo => CurrentAmmo > 0;

        [ShowInInspector]
        [ReadOnly]
        public int CurrentAmmo { get; protected set; } = 0;

        [field: SerializeField]
        public int MaxAmmo {get; protected set; } 


        [field:SerializeField]
        public UnityEvent<IWeapon> OnAmmoChanged { get; private set; } = new UnityEvent<IWeapon>();

        public bool IsEquipped { get; private set; } = false;

        [field:SerializeField]
        public UnityEvent<IEquippable> OnEquipped { get; private set; } = new UnityEvent<IEquippable>();


        [field:SerializeField]
        public UnityEvent<IEquippable> OnUnequipped { get; private set; } = new UnityEvent<IEquippable>();

        /// <summary>
        /// A reference to an object used to run coroutines for this weapon, since the weapon itself may get disabled when unequipped.
        /// </summary>
        [SerializeField]
        [RequiredIn(PrefabKind.InstanceInScene)]
        [Tooltip("Component responsible for running shooting coroutines."+
        "\n <color=red><b>Should be some object inside the player object that doesn't get disabled during gameplay (except for death).</b></color>")]
        protected MonoBehaviour _coroutineRunner;

        [SerializeField]
        [RequiredIn(PrefabKind.InstanceInScene)]
        protected ProximityWeaponZone _weaponHitbox;

        protected bool _isLoading = false;

        public void Equip()
        {
            Debug.Log($"Equipped item {gameObject.name}");
            gameObject.SetActive(true);


            OnEquipped.Invoke(this);
            // Setting to default now, change later if needed
            //SetChildLayers(0);
        }

        protected virtual void Awake()
        {
            CurrentAmmo = MaxAmmo;
        }


        /// <summary>
        /// Reloads the weapon by a percentage of its max ammo.
        /// </summary>
        public void ReloadPercent(float percentage)
        {
            int ammoToAdd = Mathf.RoundToInt(MaxAmmo * (percentage / 100f));
            Reload(ammoToAdd);
        }

        /// <summary>
        /// Reloads the weapon by a specific amount of ammo.
        /// </summary>
        public void Reload(int ammo)
        {
            CurrentAmmo = Mathf.Clamp(CurrentAmmo + ammo, 0, MaxAmmo);
            OnAmmoChanged.Invoke(this as IWeapon);
        }

        public void Unequip()
        {
            Debug.Log($"Unequipped item {gameObject.name}");
            gameObject.SetActive(false);
            OnUnequipped.Invoke(this);
        }

        public void Use()
        {
            if(!_isLoading && HasAmmo)
            {
                _coroutineRunner.StartCoroutine(FireCoroutine());
            }
        }

        protected abstract IEnumerator FireCoroutine();
    }
}