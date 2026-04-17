using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

namespace MonkeyBusiness.UI
{
    public class ScreenEffectsManager : MonoBehaviour
    {
        
        public static ScreenEffectsManager Instance { get; private set; }


        [SerializeField]
        [Tooltip("Image component used for the poop splash screen effect.")]
        Image _poopSplashScreen;

        Coroutine _poopEffectCoroutine;

        void Awake()
        {
           if(Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of ScreenEffectsManager detected! Replacing the old one.");
            }
            Instance = this;
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

        IEnumerator PoopSplashScreenRoutine(float duration)
        {
            _poopSplashScreen.gameObject.SetActive(true);
            _poopSplashScreen.color = new Color(_poopSplashScreen.color.r, _poopSplashScreen.color.g, _poopSplashScreen.color.b, 1f);   
            var tween = DOTween.ToAlpha(() => _poopSplashScreen.color, x => _poopSplashScreen.color = x, 0f, duration).SetEase(Ease.InQuart);
            yield return tween.WaitForCompletion();
            _poopSplashScreen.gameObject.SetActive(false);
            _poopEffectCoroutine = null;
        }

                
    }
}
