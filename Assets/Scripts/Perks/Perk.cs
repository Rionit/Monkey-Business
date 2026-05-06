using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MonkeyBusiness.Perks
{
    public class Perk : MonoBehaviour
    {
        public event Action<Perk> onPerkSelected;
        public event Action onClickConfirmed;

        [BoxGroup("UI"), Required, Tooltip("Image representing the perk.")] [SerializeField]
        private Image perkImage;

        [BoxGroup("UI"), Required, Tooltip("Text displaying perk name.")] [SerializeField]
        private TextMeshProUGUI perkEffectNameText;

        [BoxGroup("UI"), Required, Tooltip("Text displaying perk description.")] [SerializeField]
        private TextMeshProUGUI perkDescriptionText;

        private PerkSO perkSO;
        private bool isSelected;
        private bool isBuff;

        /// <summary>
        /// Initializes perk UI using ScriptableObject data.
        /// </summary>
        public void Setup(PerkSO perk)
        {
            if (perk == null)
            {
                Debug.LogError("PerkSO is null.");
                return;
            }

            perkSO = perk;

            perkImage.sprite = perk.nftImage;
            perkEffectNameText.text = perk.GetDisplayName();
            perkDescriptionText.text = perk.funnyDescription;
        }

        public void SetNeutral()
        {
            isSelected = false;

            if (perkSO != null)
                perkDescriptionText.text = perkSO.funnyDescription;

            Image bg = GetComponent<Image>();
            if (bg != null)
                bg.color = Color.white;
        }

        public bool IsBuff()
        {
            return isSelected && isBuff;
        }

        public void ForceResult(bool buff)
        {
            if (perkSO == null) return;

            isSelected = true;
            isBuff = buff;

            UpdateVisuals();
        }

        private void Update()
        {
            if (!isSelected || perkSO == null) return;

            if (isBuff)
                perkSO.buffEffect?.Update();
            else
                perkSO.debuffEffect?.Update();
        }
        
        public void SetInteractable(bool value)
        {
            var btn = GetComponent<Button>();
            if (btn != null)
                btn.interactable = value;
        }

        /// <summary>
        /// Updates UI based on roll result.
        /// </summary>
        private void UpdateVisuals()
        {
            perkDescriptionText.text = isBuff ? perkSO.buffEffect.GetDescription() : perkSO.debuffEffect.GetDescription();

            Image bg = GetComponent<Image>();
            if (bg != null)
            {
                bg.color = isBuff ? Color.green : Color.red;
            }
        }

        /// <summary>
        /// Applies the perk effect.
        /// </summary>
        public void ApplyEffect()
        {
            if (isBuff)
                perkSO.buffEffect.Apply();
            else
                perkSO.debuffEffect.Apply();
        }

        /// <summary>
        /// Resets the perk effect.
        /// </summary>
        public void Reset()
        {
            if (isBuff)
                perkSO.buffEffect.Reset();
            else
                perkSO.debuffEffect.Reset();
        }

        public override string ToString()
        {
            string perkName = perkSO != null ? perkSO.GetDisplayName() : base.ToString();
            string description = isBuff ? perkSO.buffEffect.GetDescription() : perkSO.debuffEffect.GetDescription();
            return isSelected ? $"{perkName} [isBuff = {isBuff}, description = {description}]" : perkName;
        }
    }
}