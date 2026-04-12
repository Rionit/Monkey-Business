using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI; // Do NOT use UIElements, that is UI Toolkit
namespace MonkeyBusiness
{
    public class InGameHUD : MonoBehaviour
    {
        [Required, BoxGroup("Crosshair Settings")]
        [SerializeField] private Image crosshair;
        [BoxGroup("Crosshair Settings"), PreviewField(50, ObjectFieldAlignment.Left), Optional, 
         InfoBox("Will default to a built-in knob if null")]
        [SerializeField] private Sprite crosshairSprite;
        [Range(1f, 20f), BoxGroup("Crosshair Settings")]
        [SerializeField] private float crosshairSize = 10f;

        void OnValidate()
        {
            if (crosshair != null)
            {
                crosshair.rectTransform.sizeDelta = new Vector2(crosshairSize, crosshairSize);
                crosshair.sprite = crosshairSprite != null ? crosshairSprite 
                    // Default to a built-in knob
                    : AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            }
        }

    }
}
