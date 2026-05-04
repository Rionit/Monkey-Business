using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using MonkeyBusiness.Combat.Health;
using MonkeyBusiness.Misc;

namespace MonkeyBusiness.Combat.Weapons
{
    public class Shotgun : ProximityBasedWeapon
    {
        [BoxGroup("Shooting stats")]
        [SerializeField]
        float _shotDuration = 0.25f;

        [BoxGroup("Shooting stats")]
        [SerializeField]
        float _shotCooldown = 0.6f;

        [BoxGroup("Shooting stats")]
        [SerializeField]
        float _damage = 270f;


        [BoxGroup("Shooting stats")]
        [SerializeField]
        [Tooltip("Maximum distance the enemy receives damage in.")]
        float _maxDistance = 5f;

        [BoxGroup("Shooting stats/Knockback")]
        [SerializeField]
        float _knockbackForce = 5f;
        
        [BoxGroup("Shooting stats/Knockback")]
        [SerializeField]
        float _knockbackDuration = 0.7f;

        [SerializeField]
        ParticleSystem _shotEffect;

        void Awake()
        {
            CurrentAmmo = MaxAmmo;
            if(_shotDuration > _shotCooldown)
            {
                Debug.LogError("Shot duration cannot be longer than shot cooldown!");
            }
            _weaponHitbox.OnTargetHit.AddListener(OnTargetHit);
        }

        void OnTargetHit(HealthController target)
        {
            var knockbackController = target.GetComponent<KnockbackController>();

            if(knockbackController != null)
            {
                Vector3 knockbackDirection = (target.transform.position - transform.position).normalized;
                knockbackController.Knockback(knockbackDirection * _knockbackForce, _knockbackDuration);
            }
            else
            {
                Debug.LogWarning("Target " + target.name + " hit by shotgun but has no KnockbackController.");
            }            

            var distance = Vector3.Distance(target.transform.position, transform.position);
            float damageModifier = 1f - Mathf.Pow(distance / _maxDistance, 2);

            Debug.Log("Calculated damage:" + _damage * damageModifier + " with distance: " + distance);
            target.TakeDamage(_damage * damageModifier);
        }

        protected override IEnumerator FireCoroutine()
        {
            _isLoading = true;
            _weaponHitbox.gameObject.SetActive(true);
            _weaponHitbox.enabled = true;
            _shotEffect.Play();

            CurrentAmmo--;

            OnAmmoChanged.Invoke(this);

            yield return new WaitForSeconds(_shotDuration);

            _weaponHitbox.gameObject.SetActive(false);
            _weaponHitbox.enabled = false;

            yield return new WaitForSeconds(_shotCooldown - _shotDuration);
            _isLoading = false;
        }
    }
}