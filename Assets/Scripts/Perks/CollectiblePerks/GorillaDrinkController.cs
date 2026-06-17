using System.Collections;
using Ami.BroAudio;
using MonkeyBusiness.Combat.Attack;
using MonkeyBusiness.Managers;
using MonkeyBusiness.Misc;
using UnityEngine;
using DG.Tweening;
using MonkeyBusiness.Combat.Weapons;
using UnityEngine.Rendering.Universal;
using Volume = UnityEngine.Rendering.Volume;

namespace MonkeyBusiness.Items
{
    public class GorillaDrinkController : CollectiblePerkController
    {
        [SerializeField] private float _meleeDamageMultiplier = 5f;
        [SerializeField] private float _meleeCooldownMultiplier = 0.25f;
        [SerializeField] private float _duration = 10f;
        
        protected override void ApplyEffect()
        {
            var meleeWeapon = GameManager.Instance.PlayerCharacter.GetComponent<PlayerMeleeWeapon>();
            meleeWeapon.AddBuff(_meleeDamageMultiplier, _meleeCooldownMultiplier);
        }

        protected override void ResetEffect()
        {
            var meleeWeapon = GameManager.Instance.PlayerCharacter.GetComponent<PlayerMeleeWeapon>();
            meleeWeapon.RemoveBuff(_meleeDamageMultiplier, _meleeCooldownMultiplier);
        }

        protected override float GetDuration()
        {
            return _duration;
        }
    }
}