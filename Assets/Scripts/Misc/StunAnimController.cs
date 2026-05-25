using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using DG.Tweening;

namespace MonkeyBusiness.Misc
{
    public class StunAnimController : MonoBehaviour
    {
        [BoxGroup("Stun VFX/Enemy")]
        [SerializeField]
        GameObject _stunVFX;

        [BoxGroup("Stun VFX")]
        [SerializeField]
        float _stunVFXFadeDuration = 0.3f;

        [BoxGroup("Stun VFX/Enemy")]
        [SerializeField]
        List<Renderer> _stunStarRenderers;

        Tween _stunVFXFadeTween;

        float _stunVFXAlpha = 0f;

        float GetStunVFXAlpha() => _stunVFXAlpha;

        float _currentAnimDuration = 0f;

        void SetStunVFXAlpha(float value)
        {
            _stunVFXAlpha = value;
            foreach(var renderer in _stunStarRenderers)
            {
                if (renderer != null)
                {
                    renderer.material.SetFloat("_DeathProgress", 1f - _stunVFXAlpha);
                }
            }
        }


        void Update()
        {
            if(_currentAnimDuration > 0f)
            {
                _currentAnimDuration -= Time.deltaTime;
            }
        }

        public void Animate(float duration)
        {
            var isStarting = _currentAnimDuration <= 0f;
            _currentAnimDuration = Mathf.Max(_currentAnimDuration, duration);

            if(isStarting)
            {
                _stunVFX.SetActive(true);
                StartCoroutine(StunAnimCoroutine());
            }
        }

        void FadeInOrOut(bool fadeIn, float duration)
        {
            if(_stunVFXFadeTween != null && _stunVFXFadeTween.IsActive())
            {
                _stunVFXFadeTween.Kill();
            }

            float startValue = fadeIn ? 0f : 1f;
            float endValue = fadeIn ? 1f : 0f;

            Debug.Log("Fading " + (fadeIn ? "in" : "out") + " stun VFX over " + duration + " seconds.");
            _stunVFXFadeTween = DOTween.To(GetStunVFXAlpha, SetStunVFXAlpha, endValue, duration).From(startValue);

            if(fadeIn && _stunVFX != null)
            {
                _stunVFX.SetActive(true);
            }
            else
            {
                _stunVFXFadeTween.OnComplete(() => _stunVFX.SetActive(false));
            }
        }

        IEnumerator StunAnimCoroutine()
        {
            FadeInOrOut(true, _stunVFXFadeDuration);

            yield return new WaitUntil(() => _currentAnimDuration <= 0f);

            FadeInOrOut(false, _stunVFXFadeDuration);
        }

    }
}
