using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using DG.Tweening;

namespace MonkeyBusiness.UI
{
    public class HealthBarController : MonoBehaviour
    {
        private enum HealthState { LOW, MEDIUM, HIGH }
        
        [BoxGroup("Health Bar Settings")]
        [ReadOnly, SerializeField] private HealthState currentState = HealthState.HIGH;
        [BoxGroup("Health Bar Settings")]
        [ReadOnly, SerializeField] private HealthState previousState = HealthState.HIGH;
        
        [BoxGroup("Health Bar Settings"), Required]
        [SerializeField] private Image outline;
        [BoxGroup("Health Bar Settings"), Required]
        [SerializeField] private Image fill;
        [BoxGroup("Health Bar Settings"), Required]
        [SerializeField] private Image backFill;
        [BoxGroup("Health Bar Settings"), Required]
        [SerializeField] private Image mask;

        [PropertyRange(0f, "@endOffset"), BoxGroup("Health Bar Settings")]
        [SerializeField] private float startOffset = 100f;

        [PropertyRange("@startOffset", "@GetImageWidth()"), BoxGroup("Health Bar Settings")]
        [SerializeField] private float endOffset = 100f;
        
        [Range(0f, 1f), BoxGroup("Health Bar Settings")]
        [SerializeField] private float value = 0.5f;

        [BoxGroup("Health Bar Settings")]
        [HorizontalGroup("Health Bar Settings/CutOffs", Title = "CutOffs", Gap = 10), HideLabel, LabelText("Mid"), PropertyRange(0f, "@highCutOff")]
        [SerializeField] private float mediumCutOff = 0.25f;
        [HorizontalGroup("Health Bar Settings/CutOffs"), HideLabel, LabelText("High"), PropertyRange("@mediumCutOff", 1f)]
        [SerializeField] private float highCutOff = 0.75f;
        
        [HorizontalGroup("Health Bar Settings/Colors", Title = "Colors", Gap = 10), HideLabel, LabelText("Low")]
        [SerializeField] private Color lowHealthColor = Color.red;
        [HorizontalGroup("Health Bar Settings/Colors"), HideLabel, LabelText("Mid")]
        [SerializeField] private Color mediumHealthColor = Color.darkOrange;
        [HorizontalGroup("Health Bar Settings/Colors"), HideLabel, LabelText("High")]
        [SerializeField] private Color highHealthColor = Color.green;
        
        [HorizontalGroup("Health Bar Settings/Sprites", Title = "Sprites", Gap = 10), HideLabel, LabelText("Low"), Required]
        [SerializeField] private Sprite lowHealthSprite;
        [HorizontalGroup("Health Bar Settings/Sprites"), HideLabel, LabelText("Mid"), Required]
        [SerializeField] private Sprite mediumHealthSprite;
        [HorizontalGroup("Health Bar Settings/Sprites"), HideLabel, LabelText("High"), Required]
        [SerializeField] private Sprite highHealthSprite;
        
        [HorizontalGroup("Health Bar Settings/Masks", Title = "Masks", Gap = 10), HideLabel, LabelText("Low"), Required]
        [SerializeField] private Sprite lowHealthMaskSprite;
        [HorizontalGroup("Health Bar Settings/Masks"), HideLabel, LabelText("Mid"), Required]
        [SerializeField] private Sprite mediumHealthMaskSprite;
        [HorizontalGroup("Health Bar Settings/Masks"), HideLabel, LabelText("High"), Required]
        [SerializeField] private Sprite highHealthMaskSprite;
        
        [SerializeField] private float valueTweenDuration = 0.25f;
        [SerializeField] private float shakeDuration = 0.2f;
        [SerializeField] private float shakeStrength = 10f;
        [SerializeField] private int shakeVibrato = 10;
        [SerializeField] private float backFillDelay = 0.1f;
        [SerializeField] private float backFillTweenDuration = 0.4f;
        
        private RectTransform rectTransform;
        private Tween valueTween;
        private Tween backFillTween;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnValidate()
        {
            if (fill != null)
            {
                previousState = currentState;
                currentState = GetState(value);
                fill.rectTransform.sizeDelta = new Vector2(GetFillSize(value), fill.rectTransform.sizeDelta.y);
                backFill.rectTransform.sizeDelta = fill.rectTransform.sizeDelta;
                fill.color = GetFillColor();
                SwitchSprites();
            }
        }

        [Button]
        public void SetValue(float newValue)
        {
            if(newValue > 1f || newValue < 0f)
                Debug.LogWarning($"Health bar value was being set outside of the 0-1 range: {newValue}");
    
            newValue = Mathf.Clamp(newValue, 0f, 1f);

            previousState = currentState;
            currentState = GetState(newValue);

            valueTween?.Kill();
            backFillTween?.Kill();

            // store start value
            float startValue = value;

            // FRONT fill (fast)
            valueTween = DOTween.To(() => value, x =>
            {
                value = x;
                UpdateBar();
            }, newValue, valueTweenDuration);

            // BACK fill (delayed + slower)
            backFillTween = DOVirtual.DelayedCall(backFillDelay, () =>
            {
                float backValue = startValue;

                DOTween.To(() => backValue, x =>
                {
                    backValue = x;
                    backFill.rectTransform.sizeDelta =
                        new Vector2(GetFillSize(backValue), backFill.rectTransform.sizeDelta.y);
                }, newValue, backFillTweenDuration);
            });

            // Shake on state change
            if (previousState != currentState)
            {
                rectTransform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato);
            }
        }

        private HealthState GetState(float value) =>
            value >= highCutOff ? HealthState.HIGH :
            value >= mediumCutOff ? HealthState.MEDIUM :
            HealthState.LOW;

        private void UpdateBar()
        {
            fill.rectTransform.sizeDelta = new Vector2(GetFillSize(value), fill.rectTransform.sizeDelta.y);
            fill.color = GetFillColor();
            SwitchSprites();
        }

        private void SwitchSprites()
        {
            if (outline == null || mask == null) return;

            switch (currentState)
            {
                case HealthState.HIGH:
                    outline.sprite = highHealthSprite;
                    mask.sprite = highHealthMaskSprite;
                    break;
                case HealthState.MEDIUM:
                    outline.sprite = mediumHealthSprite;
                    mask.sprite = mediumHealthMaskSprite;
                    break;
                case HealthState.LOW:
                    outline.sprite = lowHealthSprite;
                    mask.sprite = lowHealthMaskSprite;
                    break;
                default:
                    Debug.LogWarning("Missing current health state!");
                    break;        
            }
        }

        private Color GetFillColor()
        {
            return currentState switch
            {
                HealthState.HIGH => highHealthColor,
                HealthState.MEDIUM => mediumHealthColor,
                HealthState.LOW => lowHealthColor,
                _ => Color.pink
            };
        }

        private float GetFillSize(float value)
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            return Mathf.Lerp(startOffset, endOffset, value);
        }
        
        private float GetImageWidth()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            return rectTransform != null ? rectTransform.sizeDelta.x : 200f;
        }
    }
}
