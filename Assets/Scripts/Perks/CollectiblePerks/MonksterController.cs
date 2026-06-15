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
        public static bool isActive = false;

        public static float frenzyDurationOverride = 0f;
        
        public MusicController musicController;
        
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
        
        private float originalMouseSensitivity;
        private const float FrenzySensitivityMultiplier = 0.8f; // 20% lower

        
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
            isActive = true;
            
            GetComponent<SoundSource>().Play();
            BroAudio.SetEffect(Effect.LowPass(500, 0.5f), BroAudioType.Music); 
            
            StatsManager.Instance.PlayerMaxHealth += bonusMaxHealth;
            StatsManager.Instance.PlayerWalkSpeed += bonusWalkSpeed;
            StatsManager.Instance.PlayerHealth = StatsManager.Instance.PlayerMaxHealth;
            
            StatsManager.Instance._equipmentManager.ReloadAllWeapons();

            StaticEvents.OnPlayerMeleeAttackUsed.AddListener(Explode);
            PlayerMeleeWeapon meleeWeapon = GameManager.Instance.PlayerCharacter.GetComponent<PlayerMeleeWeapon>();
            meleeWeapon.AddBuff(2f, 0.5f);

            StatsManager.Instance.SetCameraSensitivity(0.05f);
            StatsManager.Instance.RateOfFireMultiplier = 2f;
            StatsManager.Instance.AddDamageMultiplier(penPrefab, penDamageMultiplier);
            StatsManager.Instance.AddDamageMultiplier(staplePrefab, stapleDamageMultiplier);

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
            musicController.PlayMonkster();
            BroAudio.SetEffect(Effect.LowPass(500), BroAudioType.Music); 
            yield return new WaitForSeconds(0.2f);
            BroAudio.SetEffect(Effect.ResetLowPass(5f), BroAudioType.Music);
            yield return new WaitForSeconds(frenzyDurationOverride > frenzyDuration ? frenzyDurationOverride : frenzyDuration);
            // =============================================
            // =============================================
            musicController.StopMusic(1f);
            
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
                    0.0f,
                    0.5f
                );
            }
            
            Camera.main.DOFieldOfView(60f, 0.5f);
            
            StaticEvents.OnPlayerMeleeAttackUsed.RemoveListener(Explode);
            meleeWeapon.RemoveBuff(2f, 0.5f);
            
            foreach (var item in StatsManager.Instance._equipmentManager.Items)
            {
                if (item is Rifle rifle)
                {
                    rifle.CanScope = true;
                }
            }

            StatsManager.Instance.PlayerMaxHealth -= bonusMaxHealth;
            StatsManager.Instance.PlayerWalkSpeed -= bonusWalkSpeed;

            StatsManager.Instance.SetCameraSensitivity(0.1f);
            StatsManager.Instance.RateOfFireMultiplier = 1f;
            StatsManager.Instance.RemoveDamageMultiplier(penPrefab, penDamageMultiplier);
            StatsManager.Instance.RemoveDamageMultiplier(staplePrefab, stapleDamageMultiplier);

            isActive = false;
            StaticEvents.OnMonksterStopped?.Invoke();
            
            yield return new WaitForSeconds(1.0f);
            musicController.PlayMain();
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