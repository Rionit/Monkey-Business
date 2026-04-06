using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Perk : MonoBehaviour
{
    [SerializeField] private Image perkImage;
    [SerializeField] private TextMeshProUGUI perkEffectNameText;
    [SerializeField] private TextMeshProUGUI perkDescriptionText;

    private PerkSO perkSO;
    
    /// <summary>
    /// Sets the internal values based on the
    /// Perk ScriptableObject
    /// </summary>
    public void Setup(PerkSO perk)
    {
        perkSO = perk;
        perkImage.sprite = perk.nftImage;
        perkEffectNameText.text = perk.GetDisplayName();
        perkDescriptionText.text = perk.funnyDescription;
    }
    
    /// <summary>
    /// Rolls the hidden outcome (buff or debuff).
    /// Call this AFTER player selects the perk.
    /// </summary>
    public void RollResult()
    {
        bool isBuff = Random.value > 0.5f;

        perkDescriptionText.text = isBuff ? perkSO.buffDescription : perkSO.debuffDescription;
        ApplyEffect(perkSO.effect, isBuff ? perkSO.buffValue : perkSO.debuffValue);
        GetComponent<Image>().color = isBuff ? Color.green : Color.red;
    }

    private void ApplyEffect(PerkEffect effect, float value)
    {
        switch (effect)
        {
            case PerkEffect.Health:
                // Example: PlayerStats.Instance.ModifyHealth(value);
                break;

            case PerkEffect.Speed:
                // Example: PlayerStats.Instance.ModifySpeed(value);
                break;
        }
    }
}
