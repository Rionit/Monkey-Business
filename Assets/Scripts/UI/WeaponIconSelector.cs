using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace MonkeyBusiness.UI
{
    public class WeaponIconSelector : MonoBehaviour
    {
        Tween _currentTween;

        [SerializeField]
        [Tooltip("Transform to be scaled when selected/deselected")]
        RectTransform _scaledTransform;

        [SerializeField]
        [MinMaxSlider(0.7f, 1.4f)]
        Vector2 _scaleRange = new Vector2(0.9f, 1.1f);

        [SerializeField]
        float _animationDuration = 0.3f;

        public void OnSelected()
        {
            if(_currentTween != null)
                _currentTween.Kill();


            _scaledTransform.localScale = Vector3.one * _scaleRange.x;
            _currentTween = _scaledTransform.DOScale(_scaleRange.y, _animationDuration);
        }

        public void OnDeselected()
        {
            if(_currentTween != null)
                _currentTween.Kill();

            _scaledTransform.localScale = Vector3.one * _scaleRange.y;
            _currentTween = _scaledTransform.DOScale(_scaleRange.x, _animationDuration);
        }
    }
}
