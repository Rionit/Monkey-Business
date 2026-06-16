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
    public class ChimpexController : CollectiblePerkController
    {
        [SerializeField] private float ChimpexDuration = 10f;
        
        protected override void ApplyEffect()
        {
            StatsManager.Instance.IsChimpexActive = true;
            StatsManager.Instance._equipmentManager.ReloadAllWeapons();
        }

        protected override void ResetEffect()
        {
            StatsManager.Instance.IsChimpexActive = false;
        }

        protected override float GetDuration()
        {
            return ChimpexDuration;
        }
    }
}