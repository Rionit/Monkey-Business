using System;
using System.Collections;
using MonkeyBusiness.Combat.Attack;
using MonkeyBusiness.Managers;
using MonkeyBusiness.Misc;
using UnityEngine;

namespace MonkeyBusiness.Items
{
    public class MonksterController : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        // TODO: add shotgun projectile
        [SerializeField] private GameObject staplePrefab;
        [SerializeField] private GameObject penPrefab;
        [SerializeField] private GameObject explosionPrefab;

        [Header("Monkster Frenzy")]
        [SerializeField] private float frenzyDuration = 30f;
        [SerializeField] private int bonusMaxHealth = 200;
        [SerializeField] private float bonusWalkSpeed = 10f;
        [SerializeField] private float penDamageMultiplier = 5f;
        [SerializeField] private float stapleDamageMultiplier = 5f;

        private void Start()
        {
            StaticEvents.OnMonksterPicked.AddListener(OnMonksterPicked);
        }

        private void OnMonksterPicked()
        {
            StatsManager.Instance._equipmentManager.SetCurrentItemHidden(true);
            animator.SetTrigger("MonksterPicked");

            StartCoroutine(UnhideAfterAnimation("monkster_drink"));
            StartCoroutine(MonksterFrenzy());
        }

        private IEnumerator MonksterFrenzy()
        {
            StatsManager.Instance.PlayerMaxHealth += bonusMaxHealth;
            StatsManager.Instance.PlayerWalkSpeed += bonusWalkSpeed;
            StatsManager.Instance.PlayerHealth = StatsManager.Instance.PlayerMaxHealth;

            StatsManager.Instance.AddDamageMultiplier(penPrefab, penDamageMultiplier);
            StatsManager.Instance.AddDamageMultiplier(staplePrefab, stapleDamageMultiplier);
            
            StaticEvents.OnPlayerMeleeAttackUsed.AddListener(Explode);

            yield return new WaitForSeconds(frenzyDuration);
            
            StaticEvents.OnPlayerMeleeAttackUsed.RemoveListener(Explode);

            StatsManager.Instance.PlayerMaxHealth -= bonusMaxHealth;
            StatsManager.Instance.PlayerWalkSpeed -= bonusWalkSpeed;

            StatsManager.Instance.RemoveDamageMultiplier(penPrefab, penDamageMultiplier);
            StatsManager.Instance.RemoveDamageMultiplier(staplePrefab, stapleDamageMultiplier);
        }

        void Explode()
        {
            GameObject explosion = GameObject.Instantiate(explosionPrefab, GameManager.Instance.PlayerCharacter.transform);
            explosion.GetComponent<Explosion>().targetEntityType = "Enemy";
        }
        
        private IEnumerator UnhideAfterAnimation(string stateName)
        {
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
                yield return null;

            while (animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
                yield return null;

            StatsManager.Instance._equipmentManager.SetCurrentItemHidden(false);
        }
    }
}