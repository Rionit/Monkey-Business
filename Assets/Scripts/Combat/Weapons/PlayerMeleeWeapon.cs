using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using MonkeyBusiness.Player;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using MonkeyBusiness.Combat.Health;
using MonkeyBusiness.Misc;
using DG.Tweening;
using UnityEngine.UI;


namespace MonkeyBusiness.Combat.Weapons
{
    public class PlayerMeleeWeapon : MonoBehaviour
    {

        [SerializeField]
        [BoxGroup("Melee stats")]
        float _meleeDamage = 10f;

        [SerializeField]
        [BoxGroup("Melee stats")]
        [Tooltip("For how long the attack hitbox is active")]
        float _attackDuration = 0.3f;

        [SerializeField]
        [BoxGroup("Melee stats")]
        [Tooltip("Cooldown time between attacks, in seconds")]
        float _attackCooldown = 5f;

        [SerializeField]
        [BoxGroup("Melee stats")]
        float _knockbackForce = 7f;

         [SerializeField]
        [BoxGroup("Melee stats")]
        float _knockbackDuration = 1.5f;

        PlayerInputActions _inputActions;

        [ShowInInspector]
        [ReadOnly]
        bool _onCooldown = false;

        [SerializeField]
        [RequiredIn(PrefabKind.InstanceInScene)]
        ProximityWeaponZone _attackHitbox;

        [SerializeField]
        Transform _animationTf;


        [SerializeField]
        [RequiredIn(PrefabKind.InstanceInScene)]
        ParticleSystem _attackEffect;


        InputAction _meleeAttackAction;

        [SerializeField]
        [RequiredIn(PrefabKind.InstanceInScene)]
        Image _chargeImage;

        void Awake()
        {
            _inputActions = new PlayerInputActions();
            _inputActions.Enable();
            _meleeAttackAction = InputSystem.actions.FindAction("Player/MeleeAttack");

            _attackHitbox.OnTargetHit.AddListener(OnEnemyHit);
        }

        void OnEnemyHit(HealthController enemy)
        {
            var knockbackController = enemy.GetComponent<KnockbackController>();

            if(knockbackController != null)
            {
                Vector3 knockbackDirection = (enemy.transform.position - transform.position).normalized;
                knockbackController.Knockback(knockbackDirection * _knockbackForce, _knockbackDuration);
            }
            else
            {
                Debug.LogWarning("Enemy " + enemy.name + " hit by melee weapon but has no KnockbackController.");
            }            

            enemy.TakeDamage(_meleeDamage);
        }

        IEnumerator MeleeAttackCoroutine()
        {
                _onCooldown = true;
                _attackHitbox.enabled = true;
                _attackHitbox.gameObject.SetActive(true);

                _attackEffect.Play();
                _animationTf.localRotation = Quaternion.Euler(0, 90f, 0f); // Rotate the weapon downwards for the attack animation
                _animationTf.gameObject.SetActive(true);
                var tween = _animationTf.DOLocalRotate(new Vector3(0, -90f, 0f), 0.2f).SetEase(Ease.Linear); // TODO: Make editable
                // Tween charge image to 0 with quadratic ease in-and-out
                DOTween.To(() => _chargeImage.fillAmount, x => _chargeImage.fillAmount = x, 0f, 0.5f).SetEase(Ease.InOutCubic)
                .OnComplete(() => DOTween.To(() => _chargeImage.fillAmount, x => _chargeImage.fillAmount = x, 1f, _attackCooldown - 0.5f).SetEase(Ease.Linear));

                yield return new WaitForSeconds(_attackDuration); // Duration of the attack hitbox being active
                _animationTf.gameObject.SetActive(false);

                _attackHitbox.gameObject.SetActive(false);
                _attackHitbox.enabled = false;

                yield return new WaitForSeconds(_attackCooldown - _attackDuration); // Cooldown duration
    
                _onCooldown = false;
        }

        void Update()
        {
            if(!_onCooldown && _meleeAttackAction.WasPressedThisFrame())
            {
                Debug.Log("Melee attack triggered");
                StartCoroutine(MeleeAttackCoroutine());
            }
        }

        void OnDestroy()
        {
            _inputActions.Dispose();
        }
    }
}