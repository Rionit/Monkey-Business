using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Collections;
using MonkeyBusiness.Managers;

namespace MonkeyBusiness.UI
{
    public class ScoreMultiplier : MonoBehaviour
    {
        [SerializeField]
        TMP_Text _multiplierText;

        [SerializeField]    
        Image _multiplierFill;

        [SerializeField]
        Color _baseMultiplierColor = Color.white;

        [SerializeField]
        Color _maxMultiplierColor = Color.orange;

        Sequence _multiplierTween;
        Tween _fillTween;

        float _currentFill;

        float _maxFillSize;

        float _currentFillFloor;

        float _currentSize = 1f;

        float _CurrentFill
        {
            get => _currentFill;
            set
            {
                _currentFill = value;

                _currentFillFloor = Mathf.Floor(_currentFill);

                var cappedFill = _currentFill - _currentFillFloor;
                var halfDistance = Mathf.Clamp(_currentFill*2f, 0,1);

                _multiplierFill.rectTransform.anchorMax = new Vector2(cappedFill, _multiplierFill.rectTransform.anchorMax.y);
                //_multiplierFill.rectTransform.sizeDelta = new Vector2(cappedFill * _maxFillSize, _multiplierFill.rectTransform.sizeDelta.y);
                _multiplierFill.color = new Color(Mathf.Clamp((1f - cappedFill) * 2f, 0,1), Mathf.Clamp(cappedFill*2f, 0f, 1f), 0);
            }
        }


        float _currentColorRelative;

        float _CurrentColor
        {
            get => _currentColorRelative;
            set
            {
                _currentColorRelative = value;
                _multiplierText.color = Color.Lerp(_baseMultiplierColor, _maxMultiplierColor, _currentColorRelative);
            }
        }

        void Start()
        {
            GameManager.Instance.ChangeMultiplerCallback = UpdateMultiplier;
            GameManager.Instance.ChangeCumulativeCallback = UpdateFill;

            _maxFillSize = _multiplierFill.rectTransform.sizeDelta.x;
            //_multiplierFill.rectTransform.sizeDelta = new Vector2(0, _multiplierFill.rectTransform.sizeDelta.y);
            _multiplierFill.rectTransform.anchorMax = new Vector2(0, _multiplierFill.rectTransform.anchorMax.y);
        }

        void UpdateFill(float value, bool increase)
        {
            Debug.Log("Udating damage multiplier fill with value " + value + " and increase " + increase);
            if(_fillTween != null)
            {
                _fillTween.Kill();
            }

            float endValue;
            if(increase)
            {
                // Overflow
                if(value < _currentFill - _currentFillFloor)
                {
                    endValue = _currentFillFloor + 1f + value;
                }
                else
                {
                    endValue = _currentFillFloor + value;
                }
                _fillTween = DOTween.To(() => _CurrentFill, x => _CurrentFill = x, endValue, 0.1f).SetEase(Ease.OutCubic);
            }
            else
            {
                // Underflow
                if(value > _currentFill - _currentFillFloor)
                {
                    endValue = _currentFillFloor - 1f + value;
                }
                else
                {
                    endValue = _currentFillFloor + value;
                }

                // Decrease is changed straight away
                _CurrentFill = endValue;
            }
        }

        void UpdateMultiplier(float multiplier, bool increase)
        {
            Debug.Log("Updating damage multiplier to " + multiplier + " with increase " + increase);
            _multiplierText.text = multiplier.ToString("0.0") + "x";
            if(_multiplierTween != null)
            {
                _multiplierTween.Kill();
            }

            var multiplierIndex = Array.IndexOf(GameManager.Instance.multipliers, multiplier);
            float colorLerp = multiplierIndex / (float)(GameManager.Instance.multipliers.Length - 1);
            float scaleRatio = 1f+(colorLerp*0.5f);
            _multiplierTween = DOTween.Sequence();

            _multiplierTween.Append(DOTween.To(() => _CurrentColor, x => _CurrentColor = x, colorLerp, 0.5f).SetEase(Ease.OutCubic));
            if(increase)
            {
                _multiplierText.transform.localScale = Vector3.one;
                _multiplierTween.Join(_multiplierText.transform.DOShakeScale(0.5f, Vector3.one * 1.2f).SetEase(Ease.OutCubic));
                _multiplierTween.Append(_multiplierText.transform.DOScale(Vector3.one * scaleRatio, 0.1f).SetEase(Ease.InOutCubic)); 
            }
            else 
            {
                _multiplierTween.Join(_multiplierText.transform.DOScale(Vector3.one * scaleRatio, 0.1f).SetEase(Ease.InOutCubic)); 
            }

        }

    }
}
