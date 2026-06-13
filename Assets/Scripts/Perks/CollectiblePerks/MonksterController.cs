using System;
using System.Collections;
using Ami.BroAudio;
using MonkeyBusiness.Combat.Attack;
using MonkeyBusiness.Managers;
using MonkeyBusiness.Misc;
using UnityEngine;
using DG.Tweening;
using MonkeyBusiness.Combat.Weapons;
using UnityEngine.Rendering.Universal;
using Volume = UnityEngine.Rendering.Volume;

namespace MonkeyBusiness.Items
{
    public class MonksterController : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        // TODO: add shotgun projectile
        [SerializeField] private GameObject staplePrefab;
        [SerializeField] private GameObject penPrefab;
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private Volume volume;

        [Header("Monkster Frenzy")]
        [SerializeField] private float frenzyDuration = 30f;
        [SerializeField] private int bonusMaxHealth = 200;
        [SerializeField] private float bonusWalkSpeed = 10f;
        [SerializeField] private float penDamageMultiplier = 5f;
        [SerializeField] private float stapleDamageMultiplier = 5f;

        private PaniniProjection paniniProjection;
        private DepthOfField depthOfField;
        private FilmGrain filmGrain;
        
        private void Start()
        {
            StaticEvents.OnMonksterPicked.AddListener(OnMonksterPicked);
        }

        private void OnMonksterPicked()
        {
            StatsManager.Instance._equipmentManager.CanReceiveInput = false;
            StatsManager.Instance._equipmentManager.SetCurrentItemHidden(true);
            animator.SetTrigger("MonksterPicked");

            StartCoroutine(UnhideAfterAnimation("monkster_drink"));
            StartCoroutine(MonksterFrenzy());
        }

        private IEnumerator MonksterFrenzy()
        {
            GetComponent<SoundSource>().Play();
            BroAudio.SetEffect(Effect.LowPass(500, 0.5f), BroAudioType.Music); 
            
            StatsManager.Instance.PlayerMaxHealth += bonusMaxHealth;
            StatsManager.Instance.PlayerWalkSpeed += bonusWalkSpeed;
            StatsManager.Instance.PlayerHealth = StatsManager.Instance.PlayerMaxHealth;

            StatsManager.Instance.AddDamageMultiplier(penPrefab, penDamageMultiplier);
            StatsManager.Instance.AddDamageMultiplier(staplePrefab, stapleDamageMultiplier);
            
            StaticEvents.OnPlayerMeleeAttackUsed.AddListener(Explode);

            foreach (var item in StatsManager.Instance._equipmentManager.Items)
            {
                if (item is Rifle rifle)
                {
                    rifle.CanScope = false;
                }
            }

            if (volume.profile.TryGet(out paniniProjection))
            { 
                DOTween.To(
                    () => paniniProjection.distance.value,
                    x => paniniProjection.distance.value = x,
                    1.0f,
                    0.5f
                );
            }
            
            if (volume.profile.TryGet(out depthOfField))
            { 
                DOTween.To(
                    () => depthOfField.aperture.value,
                    x => depthOfField.aperture.value = x,
                    32f,
                    0.5f
                );
            }
            
            if (volume.profile.TryGet(out filmGrain))
            {
                DOTween.To(
                    () => filmGrain.intensity.value,
                    x => filmGrain.intensity.value = x,
                    1.0f,
                    0.5f
                );
            }

            Camera.main.DOFieldOfView(150f, 0.5f);
            

            // =============================================
            // =============================================
            yield return new WaitForSeconds(0.5f);
            BroAudio.SetEffect(Effect.ResetLowPass(5f), BroAudioType.Music); 
            yield return new WaitForSeconds(frenzyDuration);
            // =============================================
            // =============================================
            
            if (volume.profile.TryGet(out paniniProjection))
            { 
                DOTween.To(
                    () => paniniProjection.distance.value,
                    x => paniniProjection.distance.value = x,
                    0.3f,
                    0.5f
                );
            }
            
            if (volume.profile.TryGet(out depthOfField))
            { 
                DOTween.To(
                    () => depthOfField.aperture.value,
                    x => depthOfField.aperture.value = x,
                    20f,
                    0.5f
                );
            }

            if (volume.profile.TryGet(out filmGrain))
            {
                DOTween.To(
                    () => filmGrain.intensity.value,
                    x => filmGrain.intensity.value = x,
                    0.3f,
                    0.5f
                );
            }
            
            Camera.main.DOFieldOfView(60f, 0.5f);
            
            StaticEvents.OnPlayerMeleeAttackUsed.RemoveListener(Explode);
            
            foreach (var item in StatsManager.Instance._equipmentManager.Items)
            {
                if (item is Rifle rifle)
                {
                    rifle.CanScope = true;
                }
            }

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
            StatsManager.Instance._equipmentManager.CanReceiveInput = true;
        }
    }
}