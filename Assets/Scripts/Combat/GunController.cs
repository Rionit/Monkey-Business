using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System;

public class GunController : MonoBehaviour
{
    /// <summary>
    /// Rate of fire in rounds per SECOND.
    /// </summary>
    [SerializeField]
    [Tooltip("Rate of fire in rounds per SECOND")]
    [Range(1f, 10f)]
    float _rateOfFire = 4f;

    /// <summary>
    /// Projectile to be fired.
    /// </summary>
    [SerializeField]
    [Tooltip("Projectile to be fired")]
    GameObject projectile;

    /// <summary>
    /// Point from which projectiles are spawned.
    /// </summary>
    [SerializeField]
    [Tooltip("Point from which projectiles are spawned")]
    Transform bulletSpawnPoint;

    /// <summary>
    /// Folder to contain spawned projectiles, for organization in the hierarchy.
    /// </summary>
    [SerializeField]
    [Tooltip("Folder to contain spawned projectiles")]
    GameObject projectileParent;

    [SerializeField]
    MonoBehaviour coroutineRunner;

    [field: SerializeField]
    public int MaxAmmo { get; private set; } = 60;

    [SerializeField]
    float reloadTime = 1f;

    float _defaultRateOfFire;

    const float HIGH_RATE = 1.3f;
    const float LOW_RATE = 0.7f;

    bool _highRate = false;
    bool _lowRate = false;
    
    public int CurrentAmmo { get; private set; }

    bool CanFire => CurrentAmmo > 0 && !_ROF;

    bool _ROF = false;

    public UnityEvent<int> OnShot;

    void Awake()
    {
        CurrentAmmo = MaxAmmo;
        _defaultRateOfFire = _rateOfFire;
    }

    public void Fire()
    {
        if (!CanFire) return;

        Debug.Log("Actually firing");

        var proj = Instantiate(projectile, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        proj.transform.parent = projectileParent.transform;
        CurrentAmmo--;
        string enemyTag = CompareTag("Player") ? "Enemy" : "Player";
        proj.GetComponent<ProjectileController>().Initialize(enemyTag);
        Debug.Log("Enemy tag for projectile ? " + enemyTag);    
        coroutineRunner.StartCoroutine(ROFCoroutine());

        OnShot.Invoke(CurrentAmmo);
    }

    public void Reload(int ammo)
    {
        CurrentAmmo = Mathf.Min(CurrentAmmo + ammo, MaxAmmo);
        OnShot.Invoke(CurrentAmmo);
    }

    public void SetHighRate(bool on)
    {
        if(on)
        {
            _rateOfFire = _defaultRateOfFire * HIGH_RATE;
            _highRate = true;
        }
        else
        {
            _highRate = false;
            if(!_lowRate)
            {
                _rateOfFire = _defaultRateOfFire;
            }
        }
    }

    public void SetLowRate(bool on)
    {
        if(on)
        {
            _rateOfFire = _defaultRateOfFire * LOW_RATE;
            _lowRate = true;
        }
        else
        {
            _lowRate = false;
            if(!_highRate)
            {
                _rateOfFire = _defaultRateOfFire;
            }
        }
    }

    IEnumerator ROFCoroutine()
    {
        _ROF = true;
        yield return new WaitForSeconds(1f / _rateOfFire);
        _ROF = false;
    }
}
