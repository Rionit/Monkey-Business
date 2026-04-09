using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Perk : MonoBehaviour
{
    public event Action<Perk> onPerkSelected;

    [BoxGroup("UI"), Required, Tooltip("Image representing the perk.")]
    [SerializeField] private Image perkImage;

    [BoxGroup("UI"), Required, Tooltip("Text displaying perk name.")]
    [SerializeField] private TextMeshProUGUI perkEffectNameText;

    [BoxGroup("UI"), Required, Tooltip("Text displaying perk description.")]
    [SerializeField] private TextMeshProUGUI perkDescriptionText;

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
        if (isSelected)
            return;

        isSelected = true;
        isBuff = Random.value > 0.5f;

        UpdateVisuals();
        ApplyEffect(perkSO.effect, GetValue());

        onPerkSelected?.Invoke(this);
    }

    /// <summary>
    /// Updates UI based on roll result.
    /// </summary>
    private void UpdateVisuals()
    {
        perkDescriptionText.text = isBuff ? perkSO.buffDescription : perkSO.debuffDescription;

        Image bg = GetComponent<Image>();
        if (bg != null)
        {
            bg.color = isBuff ? Color.green : Color.red;
        }
    }

    /// <summary>
    /// Returns the correct buff/debuff value based on result.
    /// </summary>
    public float GetValue()
    {
        return isBuff ? perkSO.buffValue : perkSO.debuffValue;
    }

    /// <summary>
    /// Applies the perk effect.
    /// </summary>
    private void ApplyEffect(PerkEffect effect, float value)
    {
        switch (effect)
        {
            case PerkEffect.Health:
                /*
                 Example: 
                 ApplyHealth(float value)
                 {
                    PlayerStats.Instance.ModifyHealth(value);
                 }
                 */
                break;

            case PerkEffect.Speed:
                /*
                 Example:
                 ApplySpeed(float value)
                 {
                    PlayerStats.Instance.ModifySpeed(value);
                 }
                 */
                break;

            default:
                Debug.LogWarning($"Unhandled perk effect: {effect}");
                break;
        }
    }

    public bool IsBuff()
    {
        return isBuff;
    }

    public override string ToString()
    {
        string perkName = perkSO != null ? perkSO.GetDisplayName() : base.ToString();
        return isSelected ? $"{perkName} [value = {GetValue()}, isBuff = {isBuff}]" : perkName;
    }
}