using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MonkeyBusiness.UI
{
    public class HealthBarController : MonoBehaviour
    {
        
        [SerializeField] private Image outline;
        [SerializeField] private Image fill;
        [SerializeField] private Image mask;

        [PropertyRange(0f, "@endOffset")]
        [SerializeField] private float startOffset = 100f;

        [PropertyRange("@startOffset", "@GetImageWidth()")]
        [SerializeField] private float endOffset = 100f;
        
        [Range(0f, 1f)]
        [SerializeField] private float value = 0.5f;

        [BoxGroup("Health Bar Stuff")]
        [HorizontalGroup("Health Bar Stuff/Colors", Title = "Colors"), HideLabel, LabelText("Low")]
        [SerializeField] private Color lowHealthColor = Color.red;
        [HorizontalGroup("Health Bar Stuff/Colors"), HideLabel, LabelText("Mid")]
        [SerializeField] private Color mediumHealthColor = Color.darkOrange;
        [HorizontalGroup("Health Bar Stuff/Colors"), HideLabel, LabelText("High")]
        [SerializeField] private Color highHealthColor = Color.green;
        
        [HorizontalGroup("Health Bar Stuff/Sprites", Title = "Sprites"), HideLabel, LabelText("Low")]
        [SerializeField] private Sprite lowHealthSprite;
        [HorizontalGroup("Health Bar Stuff/Sprites"), HideLabel, LabelText("Mid")]
        [SerializeField] private Sprite mediumHealthSprite;
        [HorizontalGroup("Health Bar Stuff/Sprites"), HideLabel, LabelText("High")]
        [SerializeField] private Sprite highHealthSprite;
        
        [HorizontalGroup("Health Bar Stuff/Masks", Title = "Masks"), HideLabel, LabelText("Low")]
        [SerializeField] private Sprite lowHealthMaskSprite;
        [HorizontalGroup("Health Bar Stuff/Masks"), HideLabel, LabelText("Mid")]
        [SerializeField] private Sprite mediumHealthMaskSprite;
        [HorizontalGroup("Health Bar Stuff/Masks"), HideLabel, LabelText("High")]
        [SerializeField] private Sprite highHealthMaskSprite;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnValidate()
        {
            if (fill != null)
            {
                fill.rectTransform.sizeDelta = new Vector2(GetFillSize(value), fill.rectTransform.sizeDelta.y);
                fill.color = GetFillColor(value);
                SwitchSprites(value);
            }
        }

        private void SwitchSprites(float value)
        {
            if (outline == null || mask == null) return;
            
            if (value >= 0.75f)
            {
                outline.sprite = highHealthSprite;
                mask.sprite = highHealthMaskSprite;
            }
            else if (value >= 0.25f)
            {
                outline.sprite = mediumHealthSprite;
                mask.sprite = mediumHealthMaskSprite;
            }
            else
            {
                outline.sprite = lowHealthSprite;
                mask.sprite = lowHealthMaskSprite;
            }
        }

        private Color GetFillColor(float value)
        {
            if(value >= 0.75f) return highHealthColor;
            else if(value >= 0.25f) return mediumHealthColor;
            else return lowHealthColor;
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
