using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using MonkeyBusiness.Misc;
using System.Diagnostics.CodeAnalysis;
using MonkeyBusiness.Managers;

namespace MonkeyBusiness.Combat.Weapons
{

    using Camera = UnityEngine.Camera;

    /// <summary>
    /// Controller of the player's weapon.
    /// </summary>
    public class Weapon : MonoBehaviour, IEquippable
    {    

        //private Transform[] _transforms = {};

        [SerializeField]
        [Tooltip("Stats of the weapon used")]
        WeaponData _data;

        [SerializeField]
        [Tooltip("Transform from which the weapon will fire projectiles.")]
        Transform _bulletSpawnPoint;

        [ShowInInspector]
        [BoxGroup("Stats")]
        [ReadOnly]
        /// <summary>
        /// Maximum ammo of the weapon.
        /// </summary>
        /// <remarks><i> Set from weapon data at Awake. </i></remarks>
        public int MaxAmmo { get; private set; } 

        /// <summary>
        /// Current ammo of the weapon.
        /// </summary>
        [ShowInInspector, ReadOnly, BoxGroup("Stats")]
        public int CurrentAmmo { get; private set; }

        /// <summary>
        /// Whether the weapon has ammo to fire or not.
        /// </summary>
        public bool HasAmmo => CurrentAmmo > 0;

        /// <summary>
        /// Whether the weapon is equipped
        /// </summary>
        public bool IsEquipped { get; private set; } = false;

        /// <summary>
        /// Event invoked when the ammo count changes, passing the new ammo count as an argument.
        /// </summary>
        public UnityEvent<Weapon> OnAmmoChanged = new();

        [SerializeField]
        UnityEvent<IEquippable> _onEquipped = new();

        [SerializeField]
        UnityEvent<IEquippable> _onUnequipped = new();

        public UnityEvent<IEquippable> OnEquipped => _onEquipped;
        
        public UnityEvent<IEquippable> OnUnequipped => _onUnequipped;

        bool _isLoading = false;

        [ShowInInspector]
        [ReadOnly]
        [Tooltip("Time between shots, in seconds. Set from weapon data at Awake.")]
        float _shootingInterval; 

        [ShowInInspector]
        [ReadOnly]
        [Tooltip("Current aim point of the weapon.")]
        Vector3 _currentAimPoint;

        /// <summary>
        /// A reference to an object used to run coroutines for this weapon, since the weapon itself may get disabled when unequipped.
        /// </summary>
        [SerializeField]
        [RequiredIn(PrefabKind.InstanceInScene)]
        [Tooltip("Component responsible for running shooting coroutines."+
        "\n <color=red><b>Should be some object inside the player object that doesn't get disabled during gameplay (except for death).</b></color>")]
        MonoBehaviour _coroutineRunner;

        const float MIN_HIT_DISTANCE = 1f;

        public void Equip()
        {
            Debug.Log($"Equipped item {gameObject.name}");
            gameObject.SetActive(true);

            OnEquipped.Invoke(this);
            // Setting to default now, change later if needed
            //SetChildLayers(0);
        }

        public void Unequip()
        {
            Debug.Log($"Unequipped item {gameObject.name}");
            gameObject.SetActive(false);
            OnUnequipped.Invoke(this);
        }

        /// <summary>
        /// Fires the weapon
        /// </summary>
        public void Use()
        {
            if(!_isLoading && HasAmmo)
            {
                _coroutineRunner.StartCoroutine(FireCoroutine());
            }
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
            OnAmmoChanged.Invoke(this);
        }

        /// <summary>
        /// Fires upon target, then waits for the shooting interval before allowing to fire again.
        /// </summary>
        /// <remarks><i>Should be run on an object that doesn't get disabled during gameplay.</i></remarks>
        IEnumerator FireCoroutine()
        {
            var projectile = Instantiate(_data.ProjectilePrefab, _bulletSpawnPoint.position, Quaternion.identity, ProjectileParentHolder.Instance.Object.transform);
            var projectileController = projectile.GetComponent<ProjectileController>();
            if (projectileController != null)
            {
                //Debug.Log("Has projectile controller");
                projectileController.Initialize("Enemy", GetAimDirection());
                projectileController.DamageMultiplier = StatsManager.Instance.GetDamageMultiplier(_data.ProjectilePrefab);
            }

            _isLoading = true;

            CurrentAmmo--;
            OnAmmoChanged.Invoke(this);
            yield return new WaitForSeconds(_shootingInterval);
            _isLoading = false;
        }

        void Awake()
        {
            //_transforms = GetComponentsInChildren<Transform>();
            MaxAmmo = _data.MaxAmmo;
            CurrentAmmo = MaxAmmo;
            
            _shootingInterval = 1f / _data.RateOfFire;

            if(_coroutineRunner == null)
            {
                _coroutineRunner = GetComponentInParent<ITargetable>() as MonoBehaviour;
                if(_coroutineRunner == null)
                {
                    Debug.LogError($"No valid coroutine runner found for weapon {gameObject.name}! Please assign one in the inspector.");
                }
            }
        }

        /// <summary>
        /// Calculates the aim direction based on the camera's forward direction and what it hits.
        /// </summary>
        /// <returns>A normalized direction vector from the weapon to the aim point.</returns>
        /// <remarks> Inspired by <a href="https://youtu.be/g3zaVxFWiKk?t=123">this video</a> </remarks>
        Vector3 GetAimDirection()
        {
            var cameraTf = Camera.main.transform;
            var farPlane = Camera.main.farClipPlane;
            var aimPoint = cameraTf.TransformPoint(Vector3.forward * farPlane);

            Ray r = new Ray(cameraTf.position, cameraTf.forward);
            if (Physics.Raycast(r, out RaycastHit hit, farPlane,
                 LayerMask.GetMask("Default", "Navigation"), QueryTriggerInteraction.Ignore)
                && hit.distance > MIN_HIT_DISTANCE) // Prevents aiming at very close objects, which can cause issues with the projectile's collider
            {
                aimPoint = hit.point;
            }

            _currentAimPoint = aimPoint;
            return (aimPoint - _bulletSpawnPoint.position).normalized;
        }
        
        void OnDrawGizmos()
        {
            if(_bulletSpawnPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(_bulletSpawnPoint.position, _currentAimPoint);
            }
        }
    }
}
