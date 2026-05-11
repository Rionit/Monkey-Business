using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Sirenix.OdinInspector;
using DG.Tweening;

namespace MonkeyBusiness.Misc
{
    public class EnemyTextureAnimator : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("List of renderers whose materials will be animated. \n\n<color=red>Ensure that the shaders use materials with <i>Enemy</i> shader!</color>")]
        List<Renderer> _animatedRenderers = new List<Renderer>();

        [SerializeField]
        [MinMaxSlider(0.0f, 1.5f)]
        Vector2 _damageAnimDuration = new Vector2(0.05f, 0.8f);

        float _deathAnimDuration = 2f;

        Tween _damageAnimTween;

        float _damageAnimProgress;

        float GetDamageAnimProgress() => _damageAnimProgress;
        void SetDamageAnimProgress(float value)
        {
            _damageAnimProgress = value;
            foreach(var renderer in _animatedRenderers)
            {
                foreach(var mat in renderer.materials)
                {
                    mat.SetFloat("_DamageProgress", _damageAnimProgress);
                }
            }
        }

        Tween _deathAnimTween;

        float _deathAnimProgress = 0f;
        float GetDeathAnimProgress() => _deathAnimProgress;
        void SetDeathAnimProgress(float value)
        {
            _deathAnimProgress = value;
            foreach(var renderer in _animatedRenderers)
            {
                foreach(var mat in renderer.materials)
                {
                    mat.SetFloat("_DeathProgress", _deathAnimProgress);
                }
            }
        }

        public IEnumerator AnimateDeath()
        {
            if(_deathAnimTween != null && _deathAnimTween.IsActive() && !_deathAnimTween.IsComplete())
            {
                _deathAnimTween.Kill();
            }
            _deathAnimTween = DOTween.To(GetDeathAnimProgress, SetDeathAnimProgress, 1f, _deathAnimDuration);
            yield return _deathAnimTween.WaitForCompletion();
        }

        public IEnumerator AnimateDamage(float damagePercent)
        {
            Debug.Log("Animating damage with percent " + damagePercent);
            if(_damageAnimTween != null && _damageAnimTween.IsActive() && !_damageAnimTween.IsComplete())
            {
                _damageAnimTween.Kill();
            }
            SetDamageAnimProgress(0f);
            var damageDuration = Mathf.Lerp(_damageAnimDuration.x, _damageAnimDuration.y, damagePercent);
            _damageAnimTween = DOTween.To(GetDamageAnimProgress, SetDamageAnimProgress, 1f, damageDuration);
            yield return _damageAnimTween.WaitForCompletion();
        }
    }
}