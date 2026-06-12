using UnityEngine;
using MonkeyBusiness.Combat.Regen;

namespace MonkeyBusiness.UI
{
    public class AmmoRegenBinder : MonoBehaviour
    {
        IAmmoRegen _ammoRegen;

        void Start()
        {
            _ammoRegen = GetComponentInChildren<IAmmoRegen>();
            if(_ammoRegen != null)
            {
                _ammoRegen.OnCollected.AddListener(ScreenEffectsManager.Instance.ShowReloadScreen);
            }
        }
    }
}
