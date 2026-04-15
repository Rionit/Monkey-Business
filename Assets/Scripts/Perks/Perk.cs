using System;
using MonkeyBusiness.Combat.Health;
using MonkeyBusiness.Managers;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor.Build.Content;
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

        /// <summary>
        /// Rolls buff/debuff and applies result.
        /// </summary>
        [Button, Tooltip("Rolls the perk result (buff or debuff).")]
        public void RollResult()
        {
            Debug.Log("Perk Clicked");
            if (isSelected)
            {
                onClickConfirmed?.Invoke();
                return;
            }

            Debug.Log("Rolling result");
            isSelected = true;
            isBuff = Random.value > 0.5f;

            UpdateVisuals();
            ApplyEffect();

            onPerkSelected?.Invoke(this);
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
        private void ApplyEffect()
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

        public bool IsBuff()
        {
            return isBuff;
        }

        public override string ToString()
        {
            string perkName = perkSO != null ? perkSO.GetDisplayName() : base.ToString();
            return isSelected ? $"{perkName} [isBuff = {isBuff}]" : perkName;
        }
    }
}