using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using DG.Tweening;
using Ami.BroAudio;

namespace MonkeyBusiness.Misc
{
    public class StunController : MonoBehaviour
    {
        enum VFXType
        {
            ENEMY,
            PLAYER,
        }

        [SerializeField]
        [Tooltip("Components that are gonna be turned off for the stun duration")]
        List<Behaviour> _stunnedComponents; 

        [BoxGroup("Stun VFX")]
        [EnumButtons]
        [SerializeField]
        VFXType _vfxType = VFXType.ENEMY;

        [ShowIf("@_vfxType == VFXType.ENEMY")]
        [BoxGroup("Stun VFX/Enemy")]
        [SerializeField]
        GameObject _stunVFX;

        [ShowIf("@_vfxType == VFXType.ENEMY")]
        [BoxGroup("Stun VFX")]
        [SerializeField]
        float _stunVFXFadeDuration = 0.3f;

        [ShowIf("@_vfxType == VFXType.ENEMY")]
        [BoxGroup("Stun VFX/Enemy")]
        [SerializeField]
        List<Renderer> _stunStarRenderers;

        Tween _stunVFXFadeTween;

        float _stunVFXAlpha = 0f;

        float GetStunVFXAlpha() => _stunVFXAlpha;

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

            if(fadeIn)
            {
                _stunVFX.SetActive(true);
            }
            else
            {
                _stunVFXFadeTween.OnComplete(() => _stunVFX.SetActive(false));
            }
        }

        /// <summary>
        /// Stuns the components for the specified duration.
        /// </summary>
        /// <param name="duration"></param>
        [BoxGroup("Debug")]
        [Button("Stun for seconds", DisplayParameters = true)]
        public void Stun(float duration)
        {
            StartCoroutine(StunEnemyCoroutine(duration));
        }

        IEnumerator StunEnemyCoroutine(float duration)
        {
            if(_vfxType == VFXType.ENEMY)
            {
                FadeInOrOut(true, _stunVFXFadeDuration);
            }

            Debug.Log("Stunned for " + duration + " seconds!");
            foreach (var component in _stunnedComponents)
            {
                if (component != null)
                    component.enabled = false;
            }

            yield return new WaitForSeconds(duration);

            foreach (var component in _stunnedComponents)
            {
                if (component != null)
                    component.enabled = true;
            }

            if(_vfxType == VFXType.ENEMY)
            {
                FadeInOrOut(false, _stunVFXFadeDuration);
            }
        }
    }
}
