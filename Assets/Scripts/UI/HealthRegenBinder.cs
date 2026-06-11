using MonkeyBusiness.Combat.Regen;
using UnityEngine;

namespace MonkeyBusiness.UI
{
    public class HealthRegenBinder : MonoBehaviour
    {

        IHealthRegen _healthRegen;

        void Start()
        {
            _healthRegen = GetComponentInChildren<IHealthRegen>();
            if(_healthRegen != null)
            {
                _healthRegen.OnCollected.AddListener(ScreenEffectsManager.Instance.ShowHealScreen);
            }
        }
    }
}
