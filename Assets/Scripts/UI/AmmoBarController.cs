using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using DG.Tweening;

namespace MonkeyBusiness.UI
{
    /// <summary>
    /// Controls a UI ammo bar with animated front/back fill, color transitions,
    /// sprite swapping based on thresholds, and shake feedback on state change.
    /// </summary>
    public class AmmoBarController : MonoBehaviour
    {
        /// <summary>
        /// Represents discrete ammo states used for visuals (color/sprite).
        /// </summary>
        private enum AmmoState { LOW, MEDIUM, HIGH }

        [SerializeField]
        private float startOffset = 0f;

        [SerializeField]
        private float endOffset = 90f;
        
        [BoxGroup("Ammo Bar Settings", centerLabel: true)]
        [ReadOnly, SerializeField, Tooltip("Current evaluated ammo state based on value.")]
        private AmmoState currentState = AmmoState.HIGH;

        [BoxGroup("Ammo Bar Settings")]
        [ReadOnly, SerializeField, Tooltip("Previous ammo state used to detect transitions.")]
        private AmmoState previousState = AmmoState.HIGH;

        [BoxGroup("Ammo Bar Settings"), Required, Tooltip("Foreground fill image (fast tween).")]
        [SerializeField] private Image fill;
        
        [Range(0f, 1f), BoxGroup("Ammo Bar Settings")]
        [SerializeField, Tooltip("Current normalized ammo value (0–1).")]
        private float value = 1.0f;

        [BoxGroup("Ammo Bar Settings")]
        [HorizontalGroup("Ammo Bar Settings/CutOffs", Title = "CutOffs", Gap = 10), HideLabel, LabelText("Mid"), PropertyRange(0f, "@highCutOff")]
        [SerializeField, Tooltip("Threshold at which ammo becomes MEDIUM.")]
        private float mediumCutOff = 0.25f;

        [HorizontalGroup("Ammo Bar Settings/CutOffs"), HideLabel, LabelText("High"), PropertyRange("@mediumCutOff", 1f)]
        [SerializeField, Tooltip("Threshold at which ammo becomes HIGH.")]
        private float highCutOff = 0.75f;
        
        [HorizontalGroup("Ammo Bar Settings/Colors", Title = "Colors", Gap = 10), HideLabel, LabelText("Low")]
        [SerializeField, Tooltip("Fill color when ammo is LOW.")]
        private Color lowAmmoColor = Color.red;

        [HorizontalGroup("Ammo Bar Settings/Colors"), HideLabel, LabelText("Mid")]
        [SerializeField, Tooltip("Fill color when ammo is MEDIUM.")]
        private Color mediumAmmoColor = Color.darkOrange;

        [HorizontalGroup("Ammo Bar Settings/Colors"), HideLabel, LabelText("High")]
        [SerializeField, Tooltip("Fill color when ammo is HIGH.")]
        private Color highAmmoColor = Color.green;
        
        
        [SerializeField, Tooltip("Duration of the front fill tween."), BoxGroup("Ammo Bar Settings/Tween Settings", centerLabel: true)]
        private float valueTweenDuration = 0.25f;
        
        private RectTransform rectTransform;
        private Tween valueTween; // Tween for the front fill animation.

        private void Awake()
        {
            rectTransform = fill.GetComponent<RectTransform>();
            startOffset = 0f;
            endOffset = rectTransform.sizeDelta.x;
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

                fill.color = GetFillColor();
            }
        }

        /// <summary>
        /// Sets the ammo value with tween animation.
        /// </summary>
        /// <param name="newValue">Normalized value between 0 and 1.</param>
        [Button(ButtonSizes.Large, ButtonStyle.Box, Expanded = true), BoxGroup("Ammo Bar Settings")]
        public void SetValue(float newValue)
        {
            if(newValue > 1f || newValue < 0f)
                Debug.LogWarning($"Ammo bar value was being set outside of the 0-1 range: {newValue}");
    
            newValue = Mathf.Clamp(newValue, 0f, 1f);

            previousState = currentState;
            currentState = GetState(newValue);

            valueTween?.Kill();

            float startValue = value;

            // FRONT fill (fast)
            valueTween = DOTween.To(() => value, x =>
            {
                value = x;
                UpdateBar();
            }, newValue, valueTweenDuration);
        }

        /// <summary>
        /// Determines the ammo state based on current value and cutoffs.
        /// </summary>
        private AmmoState GetState(float value) =>
            value >= highCutOff ? AmmoState.HIGH :
            value >= mediumCutOff ? AmmoState.MEDIUM :
            AmmoState.LOW;

        /// <summary>
        /// Updates visual elements of the ammo bar (fill, color, sprites).
        /// </summary>
        private void UpdateBar()
        {
            fill.rectTransform.sizeDelta = new Vector2(GetFillSize(value), fill.rectTransform.sizeDelta.y);
            fill.color = GetFillColor();
        }

        /// <summary>
        /// Returns the appropriate fill color for the current state.
        /// </summary>
        private Color GetFillColor()
        {
            return currentState switch
            {
                AmmoState.HIGH => highAmmoColor,
                AmmoState.MEDIUM => mediumAmmoColor,
                AmmoState.LOW => lowAmmoColor,
                _ => Color.pink
            };
        }

        /// <summary>
        /// Calculates the fill width based on value and offsets.
        /// </summary>
        private float GetFillSize(float value)
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            Debug.Log("Lerping with value: " + value + ", startOffset: " + startOffset + ", endOffset: " + endOffset); 
            Debug.Log("Fill size: " + Mathf.Lerp(startOffset, endOffset, value));
            return Mathf.Lerp(startOffset, endOffset, value);
        }


        public void OnAmmoChanged(IWeapon weapon)
        {
            float ammoPercent = (float)weapon.CurrentAmmo / weapon.MaxAmmo;
            Debug.Log("Ammo percent: " + ammoPercent);
            SetValue(ammoPercent);
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