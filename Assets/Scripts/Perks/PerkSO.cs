using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "New Perk", menuName = "Perk")]
public class PerkSO : ScriptableObject
{
    [PreviewField(75)]
    [Tooltip("Visual representation of the perk as NFT image")]
    public Sprite nftImage;

    [Tooltip("Type of effect this perk modifies (player only sees this before choosing)")]
    [LabelText("Effect Type")]
    public PerkEffect effect;

    [BoxGroup("Public Info (Shown Before Pick)")]
    [LabelText("Use Custom Effect Name")]
    [Tooltip("Enable to override the default effect name (enum) with a custom one")]
    public bool useCustomEffectName;
    
    [BoxGroup("Public Info (Shown Before Pick)")]
    [ShowIf(nameof(useCustomEffectName))]
    [LabelText("Custom Effect Name")]
    [Tooltip("Custom name shown to the player before selection")]
    public string customEffectName;

    [HideInInspector]
    public string effectName;
    
    [BoxGroup("Public Info (Shown Before Pick)")]
    [LabelText("Funny Description")]
    [Tooltip("e.g.: \"This rag is gonna get pulled!\"")]
    [TextArea]
    public string funnyDescription;

    [BoxGroup("Hidden Outcome (Revealed After Pick)")]
    [LabelText("Buff Description")]
    [Tooltip("Description shown if the player receives a positive outcome, e.g.: \"Shoots 2 additional projectiles\"")]
    [TextArea]
    public string buffDescription;

    [BoxGroup("Hidden Outcome (Revealed After Pick)")]
    [LabelText("Debuff Description")]
    [Tooltip("Description shown if the player receives a negative outcome, e.g.: \"Weapon's ammo is halved for the next round\"")]
    [TextArea]
    public string debuffDescription;

    [BoxGroup("Values")]
    [LabelText("Buff Value")]
    [Tooltip("Positive modifier value")]
    public float buffValue;

    [BoxGroup("Values")]
    [LabelText("Debuff Value")]
    [Tooltip("Negative modifier value")]
    public float debuffValue;

    [BoxGroup("Rules")]
    [LabelText("Is Unique")]
    [Tooltip("If true, this perk will not appear again after being picked")]
    public bool isUnique;
    
    /// <summary>
    /// Returns the name shown to the player before picking.
    /// </summary>
    public string GetDisplayName()
    {
        return useCustomEffectName && !string.IsNullOrEmpty(customEffectName)
            ? customEffectName
            : effect.ToString();
    }
}

public enum PerkEffect
{
    Health, 
    Speed,
    Weapon
}
