using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using MonkeyBusiness.Misc;

namespace MonkeyBusiness.Combat.Weapons
{
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

        [ShowInInspector]
        public UnityEvent<IEquippable> OnEquipped { get; private set; } = new();
        
        [ShowInInspector]
        public UnityEvent<IEquippable> OnUnequipped { get; private set; } = new();

        bool _isLoading = false;

        [ShowInInspector]
        [ReadOnly]
        [Tooltip("Time between shots, in seconds. Set from weapon data at Awake.")]
        float _shootingInterval; 

        [ShowInInspector]
        [ReadOnly]
        [Tooltip("Current aim point of the weapon.")]
        Vector3 _currentAimPoint;

        const float MIN_HIT_DISTANCE = 1f;

        public void Equip()
        {
            Debug.Log($"Equipped item {gameObject.name}");
            gameObject.SetActive(true);
<<<<<<< HEAD:Assets/Scripts/Combat/Weapon.cs

            OnEquipped.Invoke(this);
            // Setting to default now, change later if needed
            //SetChildLayers(0);
=======
            IsEquipped = true;
>>>>>>> main:Assets/Scripts/Combat/Weapons/Weapon.cs
        }

        public void Unequip()
        {
            Debug.Log($"Unequipped item {gameObject.name}");
            gameObject.SetActive(false);
<<<<<<< HEAD:Assets/Scripts/Combat/Weapon.cs
            OnUnequipped.Invoke(this);
=======
            IsEquipped = false;
>>>>>>> main:Assets/Scripts/Combat/Weapons/Weapon.cs
        }

        /// <summary>
        /// Fires the weapon
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Use()
        {
            if(!_isLoading)
            {
                StartCoroutine(FireCoroutine());
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

        IEnumerator FireCoroutine()
        {
            var projectile = Instantiate(_data.ProjectilePrefab, _bulletSpawnPoint.position, Quaternion.identity, ProjectileParentHolder.Instance.Object.transform);
            var projectileController = projectile.GetComponent<ProjectileController>();
            if (projectileController != null)
            {
                //Debug.Log("Has projectile controller");
                projectileController.Initialize("Enemy", GetAimDirection());
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
            if (
                Physics.Raycast(r, out RaycastHit hit, farPlane,
                 LayerMask.GetMask("Default", "Enemy"), QueryTriggerInteraction.Ignore)
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

        /*
        private void SetChildLayers(int layer)
        {
            foreach(Transform t in _transforms)
            {
                t.gameObject.layer = layer;
            }
        }
        */
    }
}
