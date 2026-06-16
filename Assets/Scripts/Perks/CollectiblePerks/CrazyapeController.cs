using Ami.BroAudio;
using MonkeyBusiness.Managers;
using UnityEngine;

namespace MonkeyBusiness.Items
{
    public class CrazyapeController : CollectiblePerkController
    {
        protected override void ApplyEffect()
        {
            Time.timeScale = 0.5f;
            StatsManager.Instance.PlayerWalkSpeed += 20;
            BroAudio.SetPitch(BroAudioType.Music | BroAudioType.SFX | BroAudioType.Ambience, 0.7f);
        }

        protected override void ResetEffect()
        {
            Time.timeScale = 1.0f;
            StatsManager.Instance.PlayerWalkSpeed -= 20;
            BroAudio.SetPitch(BroAudioType.Music | BroAudioType.SFX | BroAudioType.Ambience, 1.0f);
        }

        protected override float GetDuration()
        {
            return 10f;
        }
    }
}