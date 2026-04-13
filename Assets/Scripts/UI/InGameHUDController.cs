using MonkeyBusiness.UI;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
// Do NOT use UIElements, that is UI Toolkit
using TMPro;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace MonkeyBusiness
{
    public class InGameHUD : MonoBehaviour
    {
        [Required, BoxGroup("Crosshair", centerLabel: true)]
        [SerializeField] private Image crosshair;
        [BoxGroup("Crosshair"), PreviewField(50, ObjectFieldAlignment.Left), Optional, 
         InfoBox("Will default to a built-in knob if null")]
        [SerializeField] private Sprite crosshairSprite;
        [Range(1f, 20f), BoxGroup("Crosshair")]
        [SerializeField] private float crosshairSize = 10f;

        [BoxGroup("Enemy Count", centerLabel: true), Required] 
        [SerializeField] private TextMeshProUGUI enemyCountText;
        
        [BoxGroup("Health Bar", centerLabel: true), Required]
        [SerializeField] private HealthBarController healthBar;
        
        [BoxGroup("Ammo", centerLabel: true), Required]
        [SerializeField] private TextMeshProUGUI ammoText;

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

        [Button(ButtonSizes.Large, ButtonStyle.Box, Expanded = true), BoxGroup("Enemy Count")]
        public void SetEnemyCount(int value)
        {
            enemyCountText.text = value == 1 ? $"{value} Enemy Left!" : $"{value} Enemies Left!";
        }

        [Button(ButtonSizes.Large, ButtonStyle.Box, Expanded = true), BoxGroup("Health Bar")]
        public void SetHealth(float value)
        {
            healthBar.SetValue(value);
        }

        [Button(ButtonSizes.Large, ButtonStyle.Box, Expanded = true), BoxGroup("Ammo")]
        public void SetAmmo(int value)
        {
            if (value < 0)
            {
                Debug.LogError($"{value} is not a valid ammo!");
                return;
            }
            ammoText.text = $"{value}";
        }
    }
}
