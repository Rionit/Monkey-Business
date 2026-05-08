using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using MonkeyBusiness.Perks.PerkEffects;
using Sirenix.Serialization;

namespace MonkeyBusiness.Perks
{
    [CreateAssetMenu(fileName = "New Perk", menuName = "Monkey Business/Perk")]
    public class PerkSO : ScriptableObject
    {
        [BoxGroup("Public Info (Shown Before Pick)", centerLabel: true)]
        [PreviewField(75)]
        [Tooltip("Visual representation of the perk as NFT image")]
        public Sprite nftImage;

        [BoxGroup("Hidden Outcome (Revealed After Pick)")]
        [Tooltip("Determines if this perk is positive or negative")]
        [LabelText("Perk Alignment")]
        public PerkAlignment perkAlignment;

        [BoxGroup("Hidden Outcome (Revealed After Pick)")]
        [FormerlySerializedAs("buffEffect")]
        [FormerlySerializedAs("debuffEffect")]
        [Tooltip("The effect applied when this perk is selected")]
        [LabelText("Effect")]
        [SerializeReference, InlineProperty, OdinSerialize]
        public PerkEffectBase effect;

        [BoxGroup("Public Info (Shown Before Pick)")]
        [LabelText("Custom Effect Name")]
        [Tooltip("Custom name shown to the player before selection")]
        public string effectName;

        [BoxGroup("Rules")]
        [LabelText("Is Unique")]
        [Tooltip("If true, this perk will not appear again after being picked")]
        public bool isUnique;

    }

    public enum PerkAlignment
    {
        Positive,
        Negative
    }
}