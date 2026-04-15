using UnityEngine;
using MonkeyBusiness.UI;
using Sirenix.OdinInspector;
using MonkeyBusiness.Combat.Weapons;

namespace MonkeyBusiness.Effects
{
    /// <summary>
    /// Effect of the poop projectile.
    /// </summary>
    public class PoopEffect : MonoBehaviour
    {
        [BoxGroup("Components")]
        [SerializeField]
        [Required]
        ProjectileController _projectileController;
        
        [SerializeField]
        [Tooltip("Duration of the poop splash screen effect in seconds.")]
        float _poopSplashDuration = 2f;

        void Awake()
        {
            _projectileController.OnTargetHit.AddListener(InvokePoopEffect);
        }

        void InvokePoopEffect(GameObject _)
        {
            ScreenEffectsManager.Instance.ShowPoopSplashScreen(_poopSplashDuration);
        }
    }
}
