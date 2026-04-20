using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using DG.Tweening;

namespace MonkeyBusiness.UI
{
    /// <summary>
    /// Controls a UI health bar with animated front/back fill, color transitions,
    /// sprite swapping based on thresholds, and shake feedback on state change.
    /// </summary>
    public class HealthBarController : MonoBehaviour
    {
        /// <summary>
        /// Represents discrete health states used for visuals (color/sprite).
        /// </summary>
        private enum HealthState { LOW, MEDIUM, HIGH }
        
        [BoxGroup("Health Bar Settings", centerLabel: true)]
        [ReadOnly, SerializeField, Tooltip("Current evaluated health state based on value.")]
        private HealthState currentState = HealthState.HIGH;

        [BoxGroup("Health Bar Settings")]
        [ReadOnly, SerializeField, Tooltip("Previous health state used to detect transitions.")]
        private HealthState previousState = HealthState.HIGH;
        
        [BoxGroup("Health Bar Settings"), Required, Tooltip("Outline image of the health bar.")]
        [SerializeField] private Image outline;

        [BoxGroup("Health Bar Settings"), Required, Tooltip("Foreground fill image (fast tween).")]
        [SerializeField] private Image fill;

        [BoxGroup("Health Bar Settings"), Required, Tooltip("Background fill image (delayed tween for damage effect).")]
        [SerializeField] private Image backFill;

        [BoxGroup("Health Bar Settings"), Required, Tooltip("Mask image used for stylized clipping.")]
        [SerializeField] private Image mask;

        [BoxGroup("Health Bar Settings"), Required, Tooltip("Image used for the monke face.")]
        [SerializeField] private Image face;

        [PropertyRange(0f, "@endOffset"), BoxGroup("Health Bar Settings")]
        [SerializeField, Tooltip("Minimum width position of the fill (value = 0).")]
        private float startOffset = 100f;

        [PropertyRange("@startOffset", "@GetImageWidth()"), BoxGroup("Health Bar Settings")]
        [SerializeField, Tooltip("Maximum width position of the fill (value = 1).")]
        private float endOffset = 100f;
        
        [Range(0f, 1f), BoxGroup("Health Bar Settings")]
        [SerializeField, Tooltip("Current normalized health value (0–1).")]
        private float value = 1.0f;

        [BoxGroup("Health Bar Settings")]
        [HorizontalGroup("Health Bar Settings/CutOffs", Title = "CutOffs", Gap = 10), HideLabel, LabelText("Mid"), PropertyRange(0f, "@highCutOff")]
        [SerializeField, Tooltip("Threshold at which health becomes MEDIUM.")]
        private float mediumCutOff = 0.25f;

        [HorizontalGroup("Health Bar Settings/CutOffs"), HideLabel, LabelText("High"), PropertyRange("@mediumCutOff", 1f)]
        [SerializeField, Tooltip("Threshold at which health becomes HIGH.")]
        private float highCutOff = 0.75f;
        
        [HorizontalGroup("Health Bar Settings/Colors", Title = "Colors", Gap = 10), HideLabel, LabelText("Low")]
        [SerializeField, Tooltip("Fill color when health is LOW.")]
        private Color lowHealthColor = Color.red;

        [HorizontalGroup("Health Bar Settings/Colors"), HideLabel, LabelText("Mid")]
        [SerializeField, Tooltip("Fill color when health is MEDIUM.")]
        private Color mediumHealthColor = Color.darkOrange;

        [HorizontalGroup("Health Bar Settings/Colors"), HideLabel, LabelText("High")]
        [SerializeField, Tooltip("Fill color when health is HIGH.")]
        private Color highHealthColor = Color.green;
        
        [HorizontalGroup("Health Bar Settings/Sprites", Title = "Sprites", Gap = 10), HideLabel, LabelText("Low"), Required]
        [SerializeField, Tooltip("Outline sprite for LOW health.")]
        private Sprite lowHealthSprite;

        [HorizontalGroup("Health Bar Settings/Sprites"), HideLabel, LabelText("Mid"), Required]
        [SerializeField, Tooltip("Outline sprite for MEDIUM health.")]
        private Sprite mediumHealthSprite;

        [HorizontalGroup("Health Bar Settings/Sprites"), HideLabel, LabelText("High"), Required]
        [SerializeField, Tooltip("Outline sprite for HIGH health.")]
        private Sprite highHealthSprite;
        
        [HorizontalGroup("Health Bar Settings/Masks", Title = "Masks", Gap = 10), HideLabel, LabelText("Low"), Required]
        [SerializeField, Tooltip("Mask sprite for LOW health.")]
        private Sprite lowHealthMaskSprite;

        [HorizontalGroup("Health Bar Settings/Masks"), HideLabel, LabelText("Mid"), Required]
        [SerializeField, Tooltip("Mask sprite for MEDIUM health.")]
        private Sprite mediumHealthMaskSprite;

        [HorizontalGroup("Health Bar Settings/Masks"), HideLabel, LabelText("High"), Required]
        [SerializeField, Tooltip("Mask sprite for HIGH health.")]
        private Sprite highHealthMaskSprite;

        [HorizontalGroup("Health Bar Settings/Faces", Title = "Masks", Gap = 10), HideLabel, LabelText("Low"), Required]
        [SerializeField, Tooltip("Monke Face sprite for LOW health.")]
        private Sprite lowHealthFaceSprite;

        [HorizontalGroup("Health Bar Settings/Faces"), HideLabel, LabelText("Mid"), Required]
        [SerializeField, Tooltip("Monke Face sprite for MEDIUM health.")]
        private Sprite mediumHealthFaceSprite;

        [HorizontalGroup("Health Bar Settings/Faces"), HideLabel, LabelText("High"), Required]
        [SerializeField, Tooltip("Monke Face sprite for HIGH health.")]
        private Sprite highHealthFaceSprite;
        
        [SerializeField, Tooltip("Duration of the front fill tween."), BoxGroup("Health Bar Settings/Tween Settings", centerLabel: true)]
        private float valueTweenDuration = 0.25f;

        [SerializeField, Tooltip("Duration of the shake effect when state changes."), BoxGroup("Health Bar Settings/Tween Settings")]
        private float shakeDuration = 0.2f;

        [SerializeField, Tooltip("Strength of the shake effect."), BoxGroup("Health Bar Settings/Tween Settings")]
        private float shakeStrength = 10f;

        [SerializeField, Tooltip("Vibrato (frequency) of the shake effect."), BoxGroup("Health Bar Settings/Tween Settings")]
        private int shakeVibrato = 10;

        [SerializeField, Tooltip("Delay before the back fill starts animating."), BoxGroup("Health Bar Settings/Tween Settings")]
        private float backFillDelay = 0.4f;

        [SerializeField, Tooltip("Duration of the back fill tween."), BoxGroup("Health Bar Settings/Tween Settings")]
        private float backFillTweenDuration = 0.4f;
        
        private RectTransform rectTransform;
        private Tween valueTween; // Tween for the front fill animation.
        private Tween backFillTween; // Tween for the delayed back fill animation.

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// Editor-time validation to preview changes instantly in Inspector.
        /// </summary>
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

        /// <summary>
        /// Sets the health value with tween animation.
        /// </summary>
        /// <param name="newValue">Normalized value between 0 and 1.</param>
        [Button(ButtonSizes.Large, ButtonStyle.Box, Expanded = true), BoxGroup("Health Bar Settings")]
        public void SetValue(float newValue)
        {
            if(newValue > 1f || newValue < 0f)
                Debug.LogWarning($"Health bar value was being set outside of the 0-1 range: {newValue}");
    
            newValue = Mathf.Clamp(newValue, 0f, 1f);

            previousState = currentState;
            currentState = GetState(newValue);

            valueTween?.Kill();
            backFillTween?.Kill();

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

        /// <summary>
        /// Determines the health state based on current value and cutoffs.
        /// </summary>
        private HealthState GetState(float value) =>
            value >= highCutOff ? HealthState.HIGH :
            value >= mediumCutOff ? HealthState.MEDIUM :
            HealthState.LOW;

        /// <summary>
        /// Updates visual elements of the health bar (fill, color, sprites).
        /// </summary>
        private void UpdateBar()
        {
            fill.rectTransform.sizeDelta = new Vector2(GetFillSize(value), fill.rectTransform.sizeDelta.y);
            fill.color = GetFillColor();
            SwitchSprites();
        }

        /// <summary>
        /// Switches outline and mask sprites based on current health state.
        /// </summary>
        private void SwitchSprites()
        {
            if (outline == null || mask == null) return;

            switch (currentState)
            {
                case HealthState.HIGH:
                    outline.sprite = highHealthSprite;
                    mask.sprite = highHealthMaskSprite;
                    face.sprite = highHealthFaceSprite;
                    break;
                case HealthState.MEDIUM:
                    outline.sprite = mediumHealthSprite;
                    mask.sprite = mediumHealthMaskSprite;
                    face.sprite = mediumHealthFaceSprite;
                    break;
                case HealthState.LOW:
                    outline.sprite = lowHealthSprite;
                    mask.sprite = lowHealthMaskSprite;
                    face.sprite = lowHealthFaceSprite;
                    break;
                default:
                    Debug.LogWarning("Missing current health state!");
                    break;        
            }
        }

        /// <summary>
        /// Returns the appropriate fill color for the current state.
        /// </summary>
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

        /// <summary>
        /// Calculates the fill width based on value and offsets.
        /// </summary>
        private float GetFillSize(float value)
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            return Mathf.Lerp(startOffset, endOffset, value);
        }
        
        /// <summary>
        /// Returns the width of the RectTransform (used for editor constraints).
        /// </summary>
        private float GetImageWidth()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            return rectTransform != null ? rectTransform.sizeDelta.x : 200f;
        }
    }
}