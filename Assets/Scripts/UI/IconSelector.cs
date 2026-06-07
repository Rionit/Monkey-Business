using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;   
namespace MonkeyBusiness.UI
{
    public class IconSelector : MonoBehaviour
    {
        [Serializable]
        private class IconColorChange
        {
            public Image image;
            public Color selectedColor = Color.white;
            public Color unselectedColor = Color.white;
        }

        Tween _scaleTween;

        Tween _colorTween;

        [SerializeField]
        [Tooltip("Transform to be scaled when selected/deselected")]
        RectTransform _scaledTransform;

        [SerializeField]
        [MinMaxSlider(0.7f, 1.4f)]
        Vector2 _scaleRange = new Vector2(0.9f, 1.1f);

        [SerializeField]
        float _animationDuration = 0.3f;

        [SerializeField]
        [Tooltip("Can be left blank if no icons are changed")]
        List<IconColorChange> _iconColorChanges = new List<IconColorChange>();

        public void OnSelected()
        {
            if(_scaleTween != null)
                _scaleTween.Kill();


            _scaledTransform.localScale = Vector3.one * _scaleRange.x;
            _scaleTween = _scaledTransform.DOScale(_scaleRange.y, _animationDuration);

            if(_iconColorChanges.Count > 0)
            {
                if(_colorTween != null)
                    _colorTween.Kill();

                _colorTween = DOTween.Sequence();

                _colorTween =DOTween.To(() => 0f, x =>
                {
                    for(int i = 0; i < _iconColorChanges.Count; i++)
                    {
                        _iconColorChanges[i].image.color = Color.Lerp(_iconColorChanges[i].unselectedColor, _iconColorChanges[i].selectedColor, x);
                    }
                }, 1f, _animationDuration);
            }
        }

        public void OnDeselected()
        {
            if(_scaleTween != null)
                _scaleTween.Kill();

            _scaledTransform.localScale = Vector3.one * _scaleRange.y;
            _scaleTween = _scaledTransform.DOScale(_scaleRange.x, _animationDuration);

            if(_iconColorChanges.Count > 0)
            {
                if(_colorTween != null)
                    _colorTween.Kill();

                _colorTween = DOTween.Sequence();

                _colorTween =DOTween.To(() => 0f, x =>
                {
                    for(int i = 0; i < _iconColorChanges.Count; i++)
                    {
                        _iconColorChanges[i].image.color = Color.Lerp(_iconColorChanges[i].selectedColor, _iconColorChanges[i].unselectedColor, x);
                    }
                }, 1f, _animationDuration);
            }
        }
    }
}
