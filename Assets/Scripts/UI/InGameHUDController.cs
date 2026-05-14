using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
// Do NOT use UIElements, that is UI Toolkit
using TMPro;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using System.Collections;
using MonkeyBusiness.Combat.Weapons;
using System.Collections.Generic;
using MonkeyBusiness.Managers;
using MonkeyBusiness.Misc;
using DG.Tweening;
using System;

namespace MonkeyBusiness.UI
{
    public class InGameHUD : MonoBehaviour
    {   
        [Serializable]
        private class WeaponIcon
        {
            public Image foreground;
            public Image background;
        }

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

        [SerializeField] private TextMeshProUGUI scoreText;

        [SerializeField] private TextMeshProUGUI countdownText;

        [SerializeField]
        List<WeaponIcon> selectedWeaponIcons = new List<WeaponIcon>();

        [SerializeField]
        Color selectedColor = Color.white;

        [SerializeField]
        Color unselectedColor = new Color(1f,1f,1f,0.75f);

        [SerializeField]
        List<TextMeshProUGUI> perkTexts = new List<TextMeshProUGUI>();

        int currentPerkIndex = 0;

        Sequence changeWeaponSequence;

        int previousChangeIndex = -1;

        

        void Awake()
        {
            GameManager.Instance.CountdownCoroutine = AnimateCountdown;
            GameManager.OnScoreChanged.AddListener(SetScore);
        }

        void OnDestroy()
        {
            GameManager.Instance.CountdownCoroutine = null;
            GameManager.OnScoreChanged.RemoveListener(SetScore);
        }

        public void AddPerk(string perkText, bool isPermanent)
        {
            if(currentPerkIndex >= perkTexts.Count)
            {
                Debug.LogWarning("Not enough perk text fields to display all perks!");
                return;
            }
            var text = perkTexts[currentPerkIndex++];
            if(text != null)
            {
                text.text = perkText;
                text.color = isPermanent ? Color.lightGreen : Color.softRed;
            }
        }

        public void PopPerk()
        {
            if(currentPerkIndex <= 0)
            {
                Debug.LogWarning("No perks to remove!");
                return;
            }
            var text = perkTexts[--currentPerkIndex];
            if(text != null)
            {
                text.text = "";
                text.color = Color.white;
            }
        }

        void OnValidate()
        {
            if (crosshair != null)
            {
                crosshair.rectTransform.sizeDelta = new Vector2(crosshairSize, crosshairSize);
                crosshair.sprite = crosshairSprite;
            }
        }

        public void SetScore(int value)
        {
            if (value < 0)
            {
                Debug.LogError($"{value} is not a valid score!");
                return;
            }
            scoreText.text = value.ToString();
        }

        public void SetSelectedWeapon(int index)
        {
            if(index < 0 || index >= selectedWeaponIcons.Count)
            {
                Debug.LogError($"{index} is out of range for selected weapons!");
                return;
            }

            if(changeWeaponSequence != null && changeWeaponSequence.IsActive())
            {
                changeWeaponSequence.Kill();
            }

            var weaponIcon = selectedWeaponIcons[index];
            var previousWeaponIcon = previousChangeIndex >= 0 ? selectedWeaponIcons[previousChangeIndex] : null;

            changeWeaponSequence = DOTween.Sequence();
            changeWeaponSequence.Append(DOTween.To(() => weaponIcon.background.color, x => weaponIcon.background.color = x, selectedColor, 0.3f).From(unselectedColor).SetEase(Ease.OutQuad));
            changeWeaponSequence.Join(DOTween.To(() => weaponIcon.foreground.color, x => weaponIcon.foreground.color = x, selectedColor, 0.3f).From(unselectedColor).SetEase(Ease.OutQuad));
            
            if(previousWeaponIcon != null)
            {
                changeWeaponSequence.Join(DOTween.To(() => previousWeaponIcon.background.color, x => previousWeaponIcon.background.color = x, unselectedColor, 0.3f).From(selectedColor).SetEase(Ease.OutQuad));
                changeWeaponSequence.Join(DOTween.To(() => previousWeaponIcon.foreground.color, x => previousWeaponIcon.foreground.color = x, unselectedColor, 0.3f).From(selectedColor).SetEase(Ease.OutQuad));
            }

            changeWeaponSequence.OnKill(()=>
            {
                weaponIcon.background.color = selectedColor;
                weaponIcon.foreground.color = selectedColor;
                
                if(previousWeaponIcon != null)
                {
                    previousWeaponIcon.background.color = unselectedColor;
                    previousWeaponIcon.foreground.color = unselectedColor;
                }
            });

            previousChangeIndex = index;
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
            sequence.Append(countdownText.transform.DOScale(1.5f, 1f).SetEase(Ease.OutQuad).From(0f));
            sequence.Join(DOTween.To(() => countdownText.alpha, x => countdownText.alpha = x, 0f, 1f).From(1f).SetEase(Ease.InOutQuad));
            sequence.Join(DOTween.To(() => countdownText.text, x => countdownText.text = x, "3", 1f).From("3").SetEase(Ease.InFlash));
            
            // 2 ...
            sequence.Append(countdownText.transform.DOScale(1.5f, 1f).SetEase(Ease.OutQuad).From(0f));
            sequence.Join(DOTween.To(() => countdownText.alpha, x => countdownText.alpha = x, 0f, 1f).From(1f).SetEase(Ease.InOutQuad));
            sequence.Join(DOTween.To(() => countdownText.text, x => countdownText.text = x, "2", 1f).From("2").SetEase(Ease.InFlash));
            
            // 1 ...
            sequence.Append(countdownText.transform.DOScale(1.5f, 1f).SetEase(Ease.OutQuad).From(0f));
            sequence.Join(DOTween.To(() => countdownText.alpha, x => countdownText.alpha = x, 0f, 1f).From(1f).SetEase(Ease.InOutQuad));
            sequence.Join(DOTween.To(() => countdownText.text, x => countdownText.text = x, "1", 1f).From("1").SetEase(Ease.InFlash));
            
            sequence.Append(countdownText.transform.DOScale(1f, 1f).SetEase(Ease.OutQuad));
            sequence.Join(DOTween.To(() => countdownText.alpha, x => countdownText.alpha = x, 0f, 1f).From(1f).SetEase(Ease.InOutQuad));
            sequence.Join(DOTween.To(() => countdownText.text, x => countdownText.text = x, "GO!", 1f).From("GO!").SetEase(Ease.OutFlash));

            yield return sequence.WaitForCompletion(); 

            countdownText.text = "";
        }
    }
}
