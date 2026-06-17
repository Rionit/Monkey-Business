using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using MonkeyBusiness.Managers;
using MonkeyBusiness.Misc;
using Sirenix.OdinInspector;

namespace MonkeyBusiness.UI
{
    public class ScreenEffectsManager : MonoBehaviour
    {
        
        public static ScreenEffectsManager Instance { get; private set; }

        [SerializeField]
        [Tooltip("Image component used for the poop splash screen effect.")]
        Image _poopSplashScreen;

        [SerializeField]
        [Tooltip("Image component used for the hit screen effect.")]
        Image _hitScreen;

        [BoxGroup("HealScreen")]
        [SerializeField]
        Image _healScreen;

        [BoxGroup("HealScreen")]
        [SerializeField]
        GameObject _healScreenAnimations;


        [BoxGroup("ReloadScreen")]
        [SerializeField]
        Image _reloadScreen;

        [BoxGroup("ReloadScreen")]
        [SerializeField]
        GameObject _reloadScreenAnimations;

        [BoxGroup("MonksterScreen")]
        [SerializeField]
        Image _monksterScreen;

        [BoxGroup("MonksterScreen")]
        [SerializeField]
        GameObject _monksterScreenAnimations;

        Coroutine _poopEffectCoroutine;

        Coroutine _hitEffectCoroutine;

        Coroutine _healEffectCoroutine;

        Coroutine _reloadEffectCoroutine;
        
        Coroutine _monksterEffectCoroutine;

        Sequence _healSequence;
        Sequence _reloadSequence;

        void Awake()
        {
           if(Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of ScreenEffectsManager detected! Replacing the old one.");
            }
            Instance = this;
        }

        private void Start()
        {
            StaticEvents.OnPlayerHeal.AddListener(_ => ShowHealScreen());
            StaticEvents.OnCollectiblePerkPicked.AddListener(perkType =>
            {
                switch (perkType)
                {
                    case StaticEvents.CollectiblePerkType.Monkster: ShowMonksterScreen(); break;
                }
            });
            StaticEvents.OnCollectiblePerkStopped.AddListener((arg0 => arg0 = arg0));
        }

        /// <summary>
        /// Shows the poop splash screen effect for a specified duration. If the effect is already active, it will reset and start again with the new duration.
        /// </summary>
        public void ShowPoopSplashScreen(float duration)
        {
            if(_poopEffectCoroutine != null)
            {
                StopCoroutine(_poopEffectCoroutine);
            }
            _poopEffectCoroutine = StartCoroutine(PoopSplashScreenRoutine(duration));
        }

        public void ShowHitScreen()
        {
            if(_hitEffectCoroutine != null)
            {
                StopCoroutine(_hitEffectCoroutine);
            }
            _hitEffectCoroutine = StartCoroutine(DamageHealAmmoCoroutine(_hitScreen, null));
        }

        public void ShowHealScreen()
        {
            if(_healEffectCoroutine != null)
            {
                StopCoroutine(_healEffectCoroutine);
                _healScreenAnimations.SetActive(false);
            }

            _healScreenAnimations.SetActive(true);
            _healEffectCoroutine = StartCoroutine(DamageHealAmmoCoroutine(_healScreen, _healSequence));
        }
        
        public void ShowMonksterScreen()
        {
            if(_monksterEffectCoroutine != null)
            {
                StopCoroutine(_monksterEffectCoroutine);
                _monksterScreenAnimations.SetActive(false);
            }
            _monksterScreenAnimations.SetActive(true);
            _monksterEffectCoroutine = StartCoroutine(MonksterCoroutine(_monksterScreen, null));
        }

        public void ShowReloadScreen()
        {
            if(_reloadEffectCoroutine != null)
            {
                StopCoroutine(_reloadEffectCoroutine);
                _reloadScreenAnimations.SetActive(false);
            }
            _reloadScreenAnimations.SetActive(true);
            _reloadEffectCoroutine = StartCoroutine(DamageHealAmmoCoroutine(_reloadScreen, _reloadSequence));
        }

        IEnumerator PoopSplashScreenRoutine(float duration)
        {
            _poopSplashScreen.gameObject.SetActive(true);
            _poopSplashScreen.color = new Color(_poopSplashScreen.color.r, _poopSplashScreen.color.g, _poopSplashScreen.color.b, 1f);   
            var tween = DOTween.ToAlpha(() => _poopSplashScreen.color, x => _poopSplashScreen.color = x, 0f, duration).SetEase(Ease.Linear);
            yield return tween.WaitForCompletion();
            _poopSplashScreen.gameObject.SetActive(false);
            _poopEffectCoroutine = null;
        }

        IEnumerator DamageHealAmmoCoroutine(Image screen, Sequence sequence)
        {
            screen.gameObject.SetActive(true);
            screen.color = new Color(screen.color.r, screen.color.g, screen.color.b, 0f);

            if(sequence != null && sequence.IsActive())
            {
                sequence.Kill();
            }
            sequence = DOTween.Sequence();

            sequence.Append(DOTween.ToAlpha(() => screen.color, x => screen.color = x, 1f, 0.3f).SetEase(Ease.OutQuart));
            sequence.Append(DOTween.ToAlpha(() => screen.color, x => screen.color = x, 0f, .6f).SetEase(Ease.InQuart));

            yield return sequence.WaitForCompletion();
            screen.gameObject.SetActive(false);
        }
        
        IEnumerator MonksterCoroutine(Image screen, Sequence sequence)
        {
            screen.gameObject.SetActive(true);
            screen.color = new Color(screen.color.r, screen.color.g, screen.color.b, 0f);

            if(sequence != null && sequence.IsActive())
            {
                sequence.Kill();
            }
            sequence = DOTween.Sequence();

            sequence.Append(DOTween.ToAlpha(() => screen.color, x => screen.color = x, 1f, 0.3f).SetEase(Ease.OutQuart));
            sequence.AppendInterval(StatsManager.Instance.MonksterFrenzyDuration - 5f);
            sequence.Append(
                DOTween.ToAlpha(() => screen.color, x => screen.color = x, 0.2f, 0.5f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(4, LoopType.Yoyo)
            ); // 2 seconds slow
            sequence.Append(
                DOTween.ToAlpha(() => screen.color, x => screen.color = x, 0.2f, 0.25f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(12, LoopType.Yoyo)
            ); // 3 seconds fast
            sequence.Append(DOTween.ToAlpha(() => screen.color, x => screen.color = x, 0f, .6f).SetEase(Ease.InQuart));

            yield return sequence.WaitForCompletion();
            screen.gameObject.SetActive(false);
        }

    }
}
