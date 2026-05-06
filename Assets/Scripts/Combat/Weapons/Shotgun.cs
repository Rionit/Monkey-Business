using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;
using MonkeyBusiness.Combat.Health;
using MonkeyBusiness.Misc;
using System.Collections.Generic;

using Vector3 = UnityEngine.Vector3;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

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
        [Tooltip("Direction and strength of the recoil kickback applied to the shotgun when firing.")]
        Vector3 _recoilKickback = new Vector3(0f, 0f, -0.5f);

        [SerializeField]
        ParticleSystem _shotEffect;

        [SerializeField]
        Transform _distanceTarget;

        [ShowInInspector]
        Vector3 _defaultPosition;

        [SerializeField]
        string _enemyTag = "Enemy";

        List<ParticleCollisionEvent> _events;

        TweenerCore<Vector3,Vector3,VectorOptions> _shootTween;

        protected override void Awake()
        {
            _events = new List<ParticleCollisionEvent>(_shotEffect.main.maxParticles);
            _defaultPosition = transform.localPosition;
            base.Awake();
            CurrentAmmo = MaxAmmo;
            _weaponHitbox.OnTargetHit.AddListener(OnTargetHit);
        }

        public override void Unequip()
        {
            if(_shootTween != null)
            {
                DOTween.Kill(_shootTween);
            }
            transform.localPosition = _defaultPosition;
            base.Unequip();
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
            //target.TakeDamage(_damage * distanceModifier);
            target.TakeDamage(_damage);
        }

        protected override IEnumerator FireCoroutine()
        {
            _isLoading = true;
            //_weaponHitbox.gameObject.SetActive(true);
            //_weaponHitbox.enabled = true;
            _shotEffect.Play();

            CurrentAmmo--;

            OnAmmoChanged.Invoke(this);

            _shootTween = transform.DOLocalMove(_defaultPosition + _recoilKickback, 0.1f).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                _shootTween = transform.DOLocalMove(_defaultPosition, 0.2f).SetEase(Ease.InQuad);
            });
            yield return new WaitForFixedUpdate();

            //_weaponHitbox.gameObject.SetActive(false);
            // _weaponHitbox.enabled = false;

            yield return new WaitForSeconds(_shotCooldown - Time.fixedDeltaTime); // 1 frame is already waited, so subtracting it from cooldown
            _isLoading = false;
        }

        void OnParticleCollision(GameObject other)
        {
            var numHits = _shotEffect.GetCollisionEvents(other, _events);
            Debug.Log("Particle collision with " + other.name + ", " + numHits + " hits.");

            if(other.tag == _enemyTag)
            {
                var healthController = other.GetComponent<HealthController>();
                if(healthController != null)
                {
                    for(int i = 0; i < numHits; i++)
                    {
                        OnTargetHit(healthController);
                    }

                }
            }
        }

        void OnParticleTrigger()
        {
            Debug.Log("Particle trigger");
        }
    }
}