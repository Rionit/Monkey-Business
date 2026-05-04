using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
// Do NOT use UIElements, that is UI Toolkit
using TMPro;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using MonkeyBusiness.Combat.Weapons;
using MonkeyBusiness.Managers;
using MonkeyBusiness.Misc;

namespace MonkeyBusiness.UI
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
                crosshair.sprite = crosshairSprite;
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
            var max = StatsManager.Instance.PlayerMaxHealth;
            if (value < 0f || value > max)
            {
                Debug.LogWarning($"Health value is out of range 0-{max}!");
            }
            value = Mathf.Clamp(value, 0f, max);
            healthBar.SetValue(value/max);
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
        
        public void OnAmmoChanged(IWeapon weapon){
            SetAmmo(weapon.CurrentAmmo);
        }

        public void OnWeaponEquipped(IEquippable weaponEquippable)
        {
            var weapon = weaponEquippable as IWeapon;
            weapon.OnAmmoChanged.AddListener(OnAmmoChanged);
            SetAmmo(weapon.CurrentAmmo);
        }

        public void OnWeaponUnequipped(IEquippable weaponEquippable)
        {
            var weapon = weaponEquippable as IWeapon;
            weapon.OnAmmoChanged.RemoveListener(OnAmmoChanged);
        }
    }
}
