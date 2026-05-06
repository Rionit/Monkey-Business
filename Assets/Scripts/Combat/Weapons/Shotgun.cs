using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using MonkeyBusiness.Combat.Health;
using MonkeyBusiness.Misc;
using UnityEditor.Toolbars;

namespace MonkeyBusiness.Combat.Weapons
{
    public class Shotgun : ProximityBasedWeapon
    {
        enum DistanceFalloff
        {
            LINEAR,
            QUADRATIC,
            CUBIC,
        }

        [SerializeField]
        AnimationCurve _damageFalloffCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

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
        float _maxDistance = 50f;

        [SerializeField]
        [BoxGroup("Shooting stats/Knockback")]
        [Tooltip("Minimal and maximal knockback force applied to target, actual value is lerped based on distance.")]
        [MinMaxSlider(0f, 50f)]
        Vector2 _knockbackForceRange = new Vector2(5f, 20f);
        
        [BoxGroup("Shooting stats/Knockback")]
        [SerializeField]
        float _knockbackDuration = 0.7f;

        [SerializeField]
        ParticleSystem _shotEffect;

        [SerializeField]
        Transform _distanceTarget;

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

            var distance = Vector3.Distance(target.transform.position, _distanceTarget.transform.position);
            //float distanceModifier = 1f - Mathf.Pow(distance / _maxDistance, 1.5f);
            float distanceModifier = _damageFalloffCurve.Evaluate(distance / _maxDistance);

            if(knockbackController != null)
            {
                Vector3 knockbackDirection = (target.transform.position + Vector3.up * 0.3f -  _distanceTarget.transform.position).normalized;
                //Vector2 knockback2D = new Vector2(knockbackDirection.x, knockbackDirection.z);
                float knockbackForce = Mathf.Lerp(_knockbackForceRange.x, _knockbackForceRange.y, distanceModifier);
                //knockbackController.Knockback(knockback2D * knockbackForce, _knockbackDuration);

                knockbackController.Knockback(knockbackDirection * knockbackForce, _knockbackDuration, 0.925f);
            }
            else
            {
                Debug.LogWarning("Target " + target.name + " hit by shotgun but has no KnockbackController.");
            }  

            Debug.Log("Calculated damage:" + _damage * distanceModifier + " with distance: " + distance);
            target.TakeDamage(_damage * distanceModifier);
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