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

    public bool CanFire { get; private set; } = true;

    public void Fire()
    {
        if (!CanFire) return;

        Debug.Log("Actually firing");

        var proj = Instantiate(projectile, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        proj.transform.parent = projectileParent.transform;
        string enemyTag = CompareTag("Player") ? "Enemy" : "Player";
        proj.GetComponent<ProjectileController>().Initialize(enemyTag);
        Debug.Log("Enemy tag for projectile ? " + enemyTag);    
        StartCoroutine(ReloadCoroutine());
    }

    IEnumerator ReloadCoroutine()
    {
        CanFire = false;
        yield return new WaitForSeconds(1f / _rateOfFire);
        CanFire = true;
    }
}
