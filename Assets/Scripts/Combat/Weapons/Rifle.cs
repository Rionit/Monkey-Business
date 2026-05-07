using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using MonkeyBusiness.Misc;
using MonkeyBusiness.Managers;
using System.Collections.Generic;

using UnityEngine.InputSystem;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine.VFX;
using MonkeyBusiness.Player;

namespace MonkeyBusiness.Combat.Weapons
{
    using Camera = UnityEngine.Camera;
    using ProjectileHitInfo = PlayerProjectileController.ProjectileHitInfo;
    using ProjectileHitInfoComparer = PlayerProjectileController.ProjectileHitInfoComparer;
    using Random = UnityEngine.Random;

    /// <summary>
    /// Controller of the player's weapon.
    /// </summary>
    public class Rifle : MonoBehaviour, IWeapon
    {    
        static Sequence _scopeTween;

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

        [SerializeField]
        UnityEvent<IWeapon> _onAmmoChanged = new();
        
        /// <summary>
        /// Event invoked when the ammo count changes, passing the new ammo count as an argument.
        /// </summary>
        public UnityEvent<IWeapon> OnAmmoChanged => _onAmmoChanged;

        [SerializeField]
        UnityEvent<IEquippable> _onEquipped = new();

        [SerializeField]
        UnityEvent<IEquippable> _onUnequipped = new();

        public UnityEvent<IEquippable> OnEquipped => _onEquipped;
        
        public UnityEvent<IEquippable> OnUnequipped => _onUnequipped;

        [SerializeField]
        int _projectilesPerShot = 1;

        [BoxGroup("Scope")]
        [SerializeField]
        [Tooltip("Number of projectiles fired per shot.")]
        bool _usesScope;

        [BoxGroup("Scope")]
        [ShowIf("_usesScope")]
        [SerializeField]
        [Tooltip("Field of view when aiming down sights.")]
        float _scopedFOV = 30f;

        [BoxGroup("Scope")]
        [ShowIf("_usesScope")]
        [SerializeField]
        [Tooltip("Time it takes to aim down sights, in seconds.")]
        float _scopeTransitionTime = 0.2f;

        [SerializeField]
        [Tooltip("Shooting angle of the weapon, in degrees.")]
        [Range(0f, 135f)]
        float _shootingAngle = 30f; 

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

        [BoxGroup("Accuracy")]
        [SerializeField]
        [Tooltip("Accuracy radius of the aim circle at the given length. (Accuracy range)")]
        [Range(0f, 10f)]
        float _accuracyRadius;

        [BoxGroup("Accuracy")]
        [SerializeField]
        [Tooltip("Accuracy range of the aim circle at the given length.")]
        float _accuracyRange; 
        
        [SerializeField]
        [Tooltip("Direction and strength of the recoil kickback applied to the shotgun when firing.")]
        Vector3 _recoilKickback = new Vector3(0f, 0f, -0.7f);

        WeaponMover _weaponMover;

        InputAction _scopeAction;

        InputAction _moveAction;

        TweenerCore<Vector3, Vector3, VectorOptions> _recoilTween;

        float _defaultSensitivity; // Default mouse sensitivity (for non-scope)

        [SerializeField]
        [BoxGroup("Scope")]
        float _mouseSensitivityModifier = 0.65f; // Multiplier applied to mouse sensitivity when aiming down sights

        bool _isScoped = false;

        Vector3 _defaultPosition;

        Vector3 _scopePosition; 

        [SerializeField]
        [BoxGroup("Scope")]
        PlayerCamera _playerCamera;

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
            if(_usesScope && _isScoped)
            {
                if(_scopeTween != null)
                {
                    _scopeTween.Kill();
                    _scopeTween = null;
                }
                transform.localPosition = _defaultPosition; 
                Unscope();
            }

            _isScoped = false;

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

        void Scope()
        {
            Debug.Log("Scoping on " + gameObject.name);
            _playerCamera.sensitivity = _defaultSensitivity;
            Camera.main.fieldOfView = 60f;
            _scopeTween = DOTween.Sequence();

            _scopeTween.Join(Camera.main.DOFieldOfView(_scopedFOV, _scopeTransitionTime));
            _scopeTween.Join(DOTween.To(() => _playerCamera.sensitivity, x => _playerCamera.sensitivity = x, _defaultSensitivity * _mouseSensitivityModifier, _scopeTransitionTime));
            
            _weaponMover.enabled = false;
            _weaponMover.MoveTo(_scopePosition, _scopeTransitionTime, Ease.InOutQuad);

            _scopeTween.OnComplete(() => _scopeTween = null);
        }

        void Unscope()
        {
            _playerCamera.sensitivity = _defaultSensitivity * _mouseSensitivityModifier;
            Camera.main.fieldOfView = _scopedFOV;
            _scopeTween = DOTween.Sequence();
            
            _scopeTween.Join(Camera.main.DOFieldOfView(60f, _scopeTransitionTime));
            _scopeTween.Join(DOTween.To(() => _playerCamera.sensitivity, x => _playerCamera.sensitivity = x, _defaultSensitivity, _scopeTransitionTime));
            _weaponMover.MoveTo(_defaultPosition, _scopeTransitionTime, Ease.InOutQuad);
            _scopeTween.OnComplete(() => 
            {
                _scopeTween = null;
                _weaponMover.enabled = true;    
            });
        }
        
        void Update()
        {
            if(_usesScope && _scopeAction.WasPressedThisFrame())
            {
                Debug.Log("Scope action triggered for " + gameObject.name);
                if(_scopeTween != null)
                {
                    _scopeTween.Kill();
                    _scopeTween = null;
                }
                if(_isScoped)  
                    Unscope();
                else    
                    Scope();

                _isScoped = !_isScoped;
            }
        }

        /// <summary>
        /// Fires upon target, then waits for the shooting interval before allowing to fire again.
        /// </summary>
        /// <remarks><i>Should be run on an object that doesn't get disabled during gameplay.</i></remarks>
        IEnumerator FireCoroutine()
        {
            _isLoading = true;
            if(_projectilesPerShot == 1)
            {
                FireProjectile(Camera.main.transform.forward);
                //yield return new WaitForSeconds(_shootingInterval);
            }
            else
            {
                float angleStep = _shootingAngle / (_projectilesPerShot - 1);
                float startingAngle = -_shootingAngle / 2;

                for(int i = 0; i < _projectilesPerShot; i++)
                {
                    float currentAngle = startingAngle + (angleStep * i);
                    Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * Camera.main.transform.forward;
                    FireProjectile(direction);

                    //yield return new WaitForSeconds(_shootingInterval/_projectilesPerShot);
                }
            }
            var recoilTime = Mathf.Min(Mathf.Max (_shootingInterval / 3f, 0.025f), 0.1f);
            var recoilReturnTime = Mathf.Max(Mathf.Min(_shootingInterval - recoilTime, 0.5f),0f);

            _recoilTween = transform.DOLocalMove(transform.localPosition + _recoilKickback, recoilTime).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                _recoilTween = transform.DOLocalMove(_defaultPosition, recoilReturnTime).SetEase(Ease.InQuad)
                .OnComplete(() => _recoilTween = null);
            });

            yield return new WaitForSeconds(_shootingInterval);
            _isLoading = false;
        }

        void FireProjectile(Vector3 direction)
        {
            var projectile = Instantiate(_data.ProjectilePrefab, _bulletSpawnPoint.position, Quaternion.identity, ProjectileParentHolder.Instance.Object.transform);
            var projectileController = projectile.GetComponent<PlayerProjectileController>();
            if(projectileController == null)
            {
                Debug.LogError("Projectile prefab shouldn't be null!");
            }

            projectileController.DamageMultiplier = StatsManager.Instance.GetDamageMultiplier(_data.ProjectilePrefab);

            var layersToCheck = projectileController.DestroyedBy;
            var maxRange = projectileController.MaxFlyDistance;
            bool destroyedByDefault = (layersToCheck.value & (1 << LayerMask.NameToLayer("Default"))) != 0;

            float deathTime = maxRange / projectileController.Speed;

            var cameraPos = Camera.main.transform.position;
            //var aimPos = RandomAimPos();
            //var testDir = aimPos - cameraPos;
            var testDir = direction.normalized;

            //Ray cameraRay = new(Camera.main.transform.position, Camera.main.transform.forward);

            bool hitsSomething = Physics.SphereCast(
                cameraPos,
                projectileController.HitboxRadius,
                testDir,
                out RaycastHit hit,
                maxRange,
                layersToCheck,
                QueryTriggerInteraction.Collide);

            bool hitDefault = false;
            bool hitStatic = false;
            var listOfTargets = new SortedSet<ProjectileHitInfo> (new ProjectileHitInfoComparer());

            Vector3 stickPos = Vector3.positiveInfinity;
            bool hitPlayer = false;
            if(hitsSomething)
            {
                var objectHit = hit.collider.gameObject;
                var hitPos = hit.point;
                hitPlayer = objectHit.CompareTag("Player");
                hitStatic = objectHit.isStatic;
                stickPos = hit.point;

                if(!hitPlayer)
                { 
                    Debug.Log("Didn't hit player  - hit " + objectHit.name + " instead");
                    var hitDist = Vector3.Distance(cameraPos, hitPos);
                    maxRange = hitDist;
                    deathTime = hitDist / projectileController.Speed;
            
                    hitDefault = objectHit.layer == LayerMask.NameToLayer("Default");
                }            
                Debug.Log("Hit player?  " + hitPlayer);
            }   
            if(!destroyedByDefault || !hitDefault)
            {
                var targetsHit = Physics.SphereCastAll(
                    cameraPos,
                    projectileController.HitboxRadius,
                    testDir,
                    maxRange + 1f,
                    LayerMask.GetMask("Default"), QueryTriggerInteraction.Collide);

                var minDist = float.MaxValue;
                foreach(var target in targetsHit)
                {
                    var distance = Vector3.Distance(cameraPos, target.point);
                    if(destroyedByDefault && hitPlayer && !target.collider.CompareTag("Player") && distance < minDist)
                    {
                        minDist = distance;
                        maxRange = distance;
                        deathTime = distance / projectileController.Speed;
                        stickPos = target.point;
                        Debug.Log("Found new destruction point at distance " + distance);
                        hitStatic = target.collider.gameObject.isStatic;
                    }

                    if(target.collider.isTrigger && target.collider.CompareTag("Enemy"))
                    {
                        var hitTime = distance / projectileController.Speed;
                        listOfTargets.Add(new ProjectileHitInfo(target.collider.gameObject, hitTime));
                    }
                }

                
                for(int i = 0; i < listOfTargets.Count; i++)
                {
                    var target = listOfTargets.Max;
                    if(target.HitTime > deathTime)
                    {
                        listOfTargets.Remove(target);
                    }

                }
            }
            else if(hit.collider.isTrigger && hit.collider.CompareTag("Enemy"))
            {
                var distance = Vector3.Distance(cameraPos, hit.point);
                var hitTime = distance / projectileController.Speed;
                listOfTargets.Add(new ProjectileHitInfo(hit.collider.gameObject, hitTime));
            }
            Debug.Log("Has targets? " + (listOfTargets.Count > 0));
            projectileController.Initialize(GetAimDirection(testDir), deathTime, listOfTargets, hitStatic, stickPos);

            _isLoading = true;

            CurrentAmmo--;
            OnAmmoChanged.Invoke(this as IWeapon);
            
        }

        void Awake()
        {
            _weaponMover = GetComponentInChildren<WeaponMover>();
            _moveAction = InputSystem.actions.FindAction("Move");
            
            _defaultSensitivity = _playerCamera.sensitivity;
            //_transforms = GetComponentsInChildren<Transform>();
            MaxAmmo = _data.MaxAmmo;
            CurrentAmmo = MaxAmmo;
            _defaultPosition = transform.localPosition;
            _shootingInterval = 1f / _data.RateOfFire;
            _scopeAction = InputSystem.actions.FindAction("Scope");
            //_scopeAction.performed += ScopeOrUnscope;
            _scopeAction.Enable();

            if(_coroutineRunner == null)
            {
                _coroutineRunner = GetComponentInParent<ITargetable>() as MonoBehaviour;
                if(_coroutineRunner == null)
                {
                    Debug.LogError($"No valid coroutine runner found for weapon {gameObject.name}! Please assign one in the inspector.");
                }
            }
        }

        void Start()
        {
            if(_usesScope)
            {
                _scopePosition = transform.InverseTransformPoint(Camera.main.transform.position + new Vector3(0.25f, -0.35f, 0.9f)); 
            }
        }
        /// <summary>
        /// Calculates the aim direction based on the camera's forward direction and what it hits.
        /// </summary>
        /// <returns>A normalized direction vector from the weapon to the aim point.</returns>
        /// <remarks> Inspired by <a href="https://youtu.be/g3zaVxFWiKk?t=123">this video</a> </remarks>
        Vector3 GetAimDirection(Vector3 aimedDir)
        {
            var cameraTf = UnityEngine.Camera.main.transform;
            var farPlane = UnityEngine.Camera.main.farClipPlane;
            var aimPoint = cameraTf.position + aimedDir * _accuracyRange;

            /*Ray r = new Ray(cameraTf.position, cameraTf.forward);
            if (Physics.Raycast(r, out RaycastHit hit, farPlane,
                 LayerMask.GetMask("Default", "Navigation"), QueryTriggerInteraction.Ignore)
                && hit.distance > MIN_HIT_DISTANCE) // Prevents aiming at very close objects, which can cause issues with the projectile's collider
            {
                aimPoint = hit.point;
            }*/

            _currentAimPoint = aimPoint;
            return (aimPoint - _bulletSpawnPoint.position).normalized;
        }
        
        void OnDrawGizmos()
        {
            if(_bulletSpawnPoint != null)
            {
                Gizmos.color = Color.darkRed;
                Gizmos.DrawWireSphere(Camera.main.transform.position + Camera.main.transform.forward * _accuracyRange, _accuracyRadius);
            
                Vector3[] dirs = { Vector3.left, Vector3.right, Vector3.up, Vector3.down };

                foreach(var dir in dirs)
                {
                    var offset = dir * _accuracyRadius;
                    Gizmos.DrawLine(_bulletSpawnPoint.position, Camera.main.transform.position + Camera.main.transform.forward * _accuracyRange + offset);
                }
            }
        }
    }
}
