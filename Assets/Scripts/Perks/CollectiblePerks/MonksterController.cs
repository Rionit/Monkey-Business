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
    public class MonksterController : CollectiblePerkController
    {
        public MusicController musicController;

        [SerializeField] private GameObject staplePrefab;
        [SerializeField] private GameObject penPrefab;
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private Volume volume;

        [Header("Monkster Frenzy")]
        [SerializeField] private int bonusMaxHealth = 200;
        [SerializeField] private float bonusWalkSpeed = 10f;
        [SerializeField] private float penDamageMultiplier = 5f;
        [SerializeField] private float stapleDamageMultiplier = 5f;

        private PaniniProjection paniniProjection;
        private DepthOfField depthOfField;
        private FilmGrain filmGrain;

        private const float FrenzySensitivityMultiplier = 0.8f;

        protected override void ApplyEffect()
        {
            BroAudio.SetEffect(Effect.LowPass(500, 0.5f), BroAudioType.Music);

            StatsManager.Instance.PlayerMaxHealth += bonusMaxHealth;
            StatsManager.Instance.PlayerWalkSpeed += bonusWalkSpeed;
            StatsManager.Instance.PlayerHealth = StatsManager.Instance.PlayerMaxHealth;

            StatsManager.Instance._equipmentManager.ReloadAllWeapons();

            StaticEvents.OnPlayerMeleeAttackUsed.AddListener(Explode);

            var meleeWeapon = GameManager.Instance.PlayerCharacter.GetComponent<PlayerMeleeWeapon>();
            meleeWeapon.AddBuff(2f, 0.5f);

            StatsManager.Instance.SetCameraSensitivity(0.05f);
            StatsManager.Instance.RateOfFireMultiplier = 2f;
            StatsManager.Instance.AddWeaponDamageMultiplier(penPrefab, penDamageMultiplier);
            StatsManager.Instance.AddWeaponDamageMultiplier(staplePrefab, stapleDamageMultiplier);

            foreach (var item in StatsManager.Instance._equipmentManager.Items)
            {
                if (item is Rifle rifle)
                    rifle.CanScope = false;
            }

            if (volume.profile.TryGet(out paniniProjection))
            {
                DOTween.To(() => paniniProjection.distance.value,
                    x => paniniProjection.distance.value = x, 1.0f, 0.5f);
            }

            if (volume.profile.TryGet(out depthOfField))
            {
                DOTween.To(() => depthOfField.aperture.value,
                    x => depthOfField.aperture.value = x, 32f, 0.5f);
            }

            if (volume.profile.TryGet(out filmGrain))
            {
                DOTween.To(() => filmGrain.intensity.value,
                    x => filmGrain.intensity.value = x, 1.0f, 0.5f);
            }

            Camera.main.DOFieldOfView(150f, 0.5f);

            StartCoroutine(MusicIntro());
        }

        private IEnumerator MusicIntro()
        {
            yield return new WaitForSeconds(0.5f);
            musicController.PlayMonkster();
            BroAudio.SetEffect(Effect.LowPass(500), BroAudioType.Music);
            yield return new WaitForSeconds(0.2f);
            BroAudio.SetEffect(Effect.ResetLowPass(5f), BroAudioType.Music);
        }

        protected override void ResetEffect()
        {
            musicController.StopMusic(1f);

            if (volume.profile.TryGet(out paniniProjection))
            {
                DOTween.To(() => paniniProjection.distance.value,
                    x => paniniProjection.distance.value = x, 0.3f, 0.5f);
            }

            if (volume.profile.TryGet(out depthOfField))
            {
                DOTween.To(() => depthOfField.aperture.value,
                    x => depthOfField.aperture.value = x, 20f, 0.5f);
            }

            if (volume.profile.TryGet(out filmGrain))
            {
                DOTween.To(() => filmGrain.intensity.value,
                    x => filmGrain.intensity.value = x, 0f, 0.5f);
            }

            Camera.main.DOFieldOfView(60f, 0.5f);

            StaticEvents.OnPlayerMeleeAttackUsed.RemoveListener(Explode);

            var meleeWeapon = GameManager.Instance.PlayerCharacter.GetComponent<PlayerMeleeWeapon>();
            meleeWeapon.RemoveBuff(2f, 0.5f);

            foreach (var item in StatsManager.Instance._equipmentManager.Items)
            {
                if (item is Rifle rifle)
                    rifle.CanScope = true;
            }

            StatsManager.Instance.PlayerMaxHealth -= bonusMaxHealth;
            StatsManager.Instance.PlayerWalkSpeed -= bonusWalkSpeed;

            StatsManager.Instance.SetCameraSensitivity(0.1f);
            StatsManager.Instance.RateOfFireMultiplier = 1f;
            StatsManager.Instance.RemoveWeaponDamageMultiplier(penPrefab, penDamageMultiplier);
            StatsManager.Instance.RemoveWeaponDamageMultiplier(staplePrefab, stapleDamageMultiplier);

            StartCoroutine(RestartMusic());
        }

        private IEnumerator RestartMusic()
        {
            yield return new WaitForSeconds(1f);
            musicController.PlayMain();
        }

        protected override float GetDuration()
        {
            return StatsManager.Instance.MonksterFrenzyDuration;
        }

        private void Explode()
        {
            var explosion = Instantiate(explosionPrefab, GameManager.Instance.PlayerCharacter.transform);
            explosion.GetComponent<Explosion>().targetEntityType = "Enemy";
        }
    }
}