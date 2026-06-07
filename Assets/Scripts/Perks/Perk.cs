using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonkeyBusiness.Perks
{
    public class Perk : MonoBehaviour
    {
        [BoxGroup("UI"), Required]
        [SerializeField] private Image perkImage;

        [BoxGroup("UI"), Required]
        [SerializeField] private TextMeshProUGUI perkEffectNameText;

        [BoxGroup("UI"), Required]
        [SerializeField] private TextMeshProUGUI perkDescriptionText;

        [BoxGroup("UI"), Required]
        [SerializeField] private GameObject confirmLabel;

        [SerializeField] public PerkSO perkSO;
        private bool isSelected;

        public void Setup(PerkSO perk)
        {
            if (perk == null)
            {
                Debug.LogError("PerkSO is null.");
                return;
            }

            perkSO = perk;

            perkImage.sprite = perk.nftImage;
            perkEffectNameText.text = perk.effectName;
            perkDescriptionText.text = perk.effect.GetDescription();

            if (confirmLabel != null)
                confirmLabel.SetActive(false);

            UpdateBackground(Color.white);
        }

        public void SetNeutral()
        {
            isSelected = false;

            if (confirmLabel != null)
                confirmLabel.SetActive(false);

            UpdateBackground(Color.white);
        }

        public void ForceSelect()
        {
            if (perkSO == null)
                return;

            isSelected = true;

            if (confirmLabel != null)
                confirmLabel.SetActive(true);

            UpdateBackground(
                perkSO.perkAlignment == PerkAlignment.Positive
                    ? Color.green
                    : Color.red
            );
        }

        private void Update()
        {
            if (!isSelected || perkSO == null || perkSO.effect == null)
                return;

            perkSO.effect.Update();
        }

        public void SetInteractable(bool value)
        {
            var btn = GetComponent<Button>();

            if (btn != null)
                btn.interactable = value;
        }

        /*
         TODO: CHECK IF I (F.D.) ACCIDENTALLY DIDN'T LEAVE
         return; LINE IN THIS FUNCTION XD
        */
        public void ApplyEffect()
        {
            Debug.Log("Applying effect " + this.name);
            perkSO.effect?.Apply();
        }

        public void Reset()
        {
            perkSO.effect?.Reset();
        }

        private void UpdateBackground(Color color)
        {
            Image bg = GetComponent<Image>();

            if (bg != null)
                bg.color = color;
        }

        public override string ToString()
        {
            string perkName = perkSO != null
                ? perkSO.effectName
                : base.ToString();

            string description = perkSO != null && perkSO.effect != null
                ? perkSO.effect.GetDescription()
                : "No Effect";

            return isSelected
                ? $"{perkName} [{perkSO.perkAlignment}, description = {description}]"
                : perkName;
        }
    }
}