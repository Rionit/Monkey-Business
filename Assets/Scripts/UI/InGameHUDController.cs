using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
// Do NOT use UIElements, that is UI Toolkit
using TMPro;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using System.Collections;
using MonkeyBusiness.Combat.Weapons;
using MonkeyBusiness.Managers;
using MonkeyBusiness.Misc;
using DG.Tweening;

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

        [SerializeField] private TextMeshProUGUI wavesCompletedText;

        void Start()
        {
            GameManager.Instance.CountdownCoroutine = AnimateCountdown;
        }

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
        
        public void SetWavesCompleted(int value)
        {
            if (value < 0)
            {
                Debug.LogError($"{value} is not a valid wave count!");
                return;
            }
            wavesCompletedText.text = value.ToString();
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

        public IEnumerator AnimateCountdown()
        {

            var sequence = DOTween.Sequence();
            // 3 ...
            sequence.Append(wavesCompletedText.transform.DOScale(1.5f, 1f).SetEase(Ease.OutQuad).From(0f));
            sequence.Join(DOTween.To(() => wavesCompletedText.alpha, x => wavesCompletedText.alpha = x, 0f, 1f).From(1f).SetEase(Ease.InOutQuad));
            sequence.Join(DOTween.To(() => wavesCompletedText.text, x => wavesCompletedText.text = x, "2", 1f).From("3").SetEase(Ease.InFlash));
            
            // 2 ...
            sequence.Append(wavesCompletedText.transform.DOScale(1.5f, 1f).SetEase(Ease.OutQuad).From(0f));
            sequence.Join(DOTween.To(() => wavesCompletedText.alpha, x => wavesCompletedText.alpha = x, 0f, 1f).From(1f).SetEase(Ease.InOutQuad));
            sequence.Join(DOTween.To(() => wavesCompletedText.text, x => wavesCompletedText.text = x, "1", 1f).From("2").SetEase(Ease.OutFlash));
            
            // 1 ...
            sequence.Append(wavesCompletedText.transform.DOScale(1.5f, 1f).SetEase(Ease.OutQuad).From(0f));
            sequence.Join(DOTween.To(() => wavesCompletedText.alpha, x => wavesCompletedText.alpha = x, 0f, 1f).From(1f).SetEase(Ease.InOutQuad));
            sequence.Join(DOTween.To(() => wavesCompletedText.text, x => wavesCompletedText.text = x, "GO!", 1f).From("1").SetEase(Ease.OutFlash));
            
            sequence.Append(wavesCompletedText.transform.DOScale(1f, 1f).SetEase(Ease.OutQuad));
            sequence.Join(DOTween.To(() => wavesCompletedText.alpha, x => wavesCompletedText.alpha = x, 0f, 1f).From(1f).SetEase(Ease.InOutQuad));
            sequence.Join(DOTween.To(() => wavesCompletedText.text, x => wavesCompletedText.text = x, string.Empty, 1f).From("GO!").SetEase(Ease.OutFlash));

            yield return sequence.WaitForCompletion(); 
        }
    }
}
