using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using MonkeyBusiness.Misc;

namespace MonkeyBusiness.Combat
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
        Transform _firePoint;

        [field:ShowInInspector]
        [field: BoxGroup("Stats")]
        [field: ReadOnly]
        /// <summary>
        /// Maximum ammo of the weapon.
        /// </summary>
        /// <remarks><i> Set from weapon data at Awake. </i></remarks>
        public int MaxAmmo { get; private set; } 

        /// <summary>
        /// Current ammo of the weapon.
        /// </summary>
        public int CurrentAmmo { get; private set; }

        /// <summary>
        /// Whether the weapon has ammo to fire or not.
        /// </summary>
        public bool HasAmmo => CurrentAmmo > 0;

        /// <summary>
        /// Event invoked when the ammo count changes, passing the new ammo count as an argument.
        /// </summary>
        public UnityEvent<int> OnAmmoChanged;

        bool _isLoading = false;

        public void Equip()
        {
            Debug.Log($"Equipped item {gameObject.name}");
            gameObject.SetActive(true);

            // Setting to default now, change later if needed
            //SetChildLayers(0);
        }

        public void Unequip()
        {
            Debug.Log($"Unequipped item {gameObject.name}");
            //SetChildLayers(LayerMask.NameToLayer("UnequippedItem"));
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Fires the weapon
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Use()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator FireCoroutine()
        {
            var projectile = Instantiate(_data.ProjectilePrefab, _firePoint.position, Quaternion.identity, GameManager.Instance.ProjectileParent.transform);
            var projectileController = projectile.GetComponent<ProjectileController>();
            if (projectileController != null)
            {
                Debug.Log("Has projectile controller");
                projectileController.Initialize("Player", (target.transform.position - _firePoint.position));
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        void Awake()
        {
            //_transforms = GetComponentsInChildren<Transform>();
        }

        // Update is called once per frame
        void Update()
        {
        
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
