using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using MonkeyBusiness.Misc;

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

        [SerializeField]
        Image _healScreen;

        [SerializeField]
        Image _reloadScreen;

        [SerializeField]
        Image _monksterScreen;
        
        Coroutine _poopEffectCoroutine;

        Coroutine _hitEffectCoroutine;

        Coroutine _healEffectCoroutine;

        Coroutine _reloadEffectCoroutine;
        
        Coroutine _monksterEffectCoroutine;

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
            StaticEvents.OnMonksterPicked.AddListener(() => ShowMonksterScreen());
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
            _hitEffectCoroutine = StartCoroutine(DamageHealAmmoCoroutine(_hitScreen));
        }

        public void ShowHealScreen()
        {
            if(_healEffectCoroutine != null)
            {
                StopCoroutine(_healEffectCoroutine);
            }
            _healEffectCoroutine = StartCoroutine(DamageHealAmmoCoroutine(_healScreen));
        }
        
        public void ShowMonksterScreen()
        {
            if(_monksterEffectCoroutine != null)
            {
                StopCoroutine(_monksterEffectCoroutine);
            }
            _monksterEffectCoroutine = StartCoroutine(DamageHealAmmoCoroutine(_monksterScreen));
        }

        public void ShowReloadScreen()
        {
            if(_reloadEffectCoroutine != null)
            {
                StopCoroutine(_reloadEffectCoroutine);
            }
            _reloadEffectCoroutine = StartCoroutine(DamageHealAmmoCoroutine(_reloadScreen));
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

        IEnumerator DamageHealAmmoCoroutine(Image screen)
        {
            screen.gameObject.SetActive(true);
            screen.color = new Color(screen.color.r, screen.color.g, screen.color.b, 0f);

            var sequence = DOTween.Sequence();

            sequence.Append(DOTween.ToAlpha(() => screen.color, x => screen.color = x, 1f, 0.3f).SetEase(Ease.OutQuart));
            sequence.Append(DOTween.ToAlpha(() => screen.color, x => screen.color = x, 0f, .6f).SetEase(Ease.InQuart));

            yield return sequence.WaitForCompletion();
            screen.gameObject.SetActive(false);
        }

    }
}
