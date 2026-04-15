using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace MonkeyBusiness.Perks
{
    [CreateAssetMenu(fileName = "New Perk", menuName = "Monkey Business/Perk")]
    public class PerkSO : ScriptableObject
    {
        [BoxGroup("Public Info (Shown Before Pick)", centerLabel: true)]
        [PreviewField(75)] [Tooltip("Visual representation of the perk as NFT image")]
        public Sprite nftImage;

        [BoxGroup("Hidden Outcome (Revealed After Pick)", centerLabel: true)]
        [FormerlySerializedAs("effect")]
        [Tooltip("Type of effect this perk modifies (player only sees this before choosing)")]
        [LabelText("Effect Type")]
        public PerkEffectType effectType;

        [BoxGroup("Hidden Outcome (Revealed After Pick)")]
        [Tooltip("The buff effect that will be applied after selection")]
        [LabelText("Buff Effect")]
        [SerializeReference, InlineProperty]
        public PerkEffectBase buffEffect;
        
        [BoxGroup("Hidden Outcome (Revealed After Pick)")]
        [Tooltip("The debuff effect that will be applied after selection")]
        [LabelText("Debuff Effect")]
        [SerializeReference, InlineProperty]
        public PerkEffectBase debuffEffect;
        
        [BoxGroup("Public Info (Shown Before Pick)")]
        [LabelText("Use Custom Effect Name")]
        [Tooltip("Enable to override the default effect name (enum) with a custom one")]
        public bool useCustomEffectName;

        [BoxGroup("Public Info (Shown Before Pick)")]
        [ShowIf(nameof(useCustomEffectName))]
        [LabelText("Custom Effect Name")]
        [Tooltip("Custom name shown to the player before selection")]
        public string customEffectName;

        [HideInInspector] public string effectName;

        [BoxGroup("Public Info (Shown Before Pick)")]
        [LabelText("Funny Description")]
        [Tooltip("e.g.: \"This rag is gonna get pulled!\"")]
        [TextArea]
        public string funnyDescription;

        [BoxGroup("Rules")]
        [LabelText("Is Unique")]
        [Tooltip("If true, this perk will not appear again after being picked")]
        public bool isUnique; // TODO: Implement this

        /// <summary>
        /// Returns the name shown to the player before picking.
        /// </summary>
        public string GetDisplayName()
        {
            return useCustomEffectName && !string.IsNullOrEmpty(customEffectName)
                ? customEffectName
                : effectType.ToString();
        }
    }

    public enum PerkEffectType
    {
        Health,
        Speed,
        Weapon
    }
}
