using UnityEngine;
using MonkeyBusiness.Player;

namespace MonkeyBusiness.UI
{
    public class RopeAvailabilityChecker : MonoBehaviour
    {
        [SerializeField]
        PlayerCharacter _player;

        [SerializeField]
        IconSelector _ropeIcon;

        [SerializeField]
        float _checkInterval = 0.25f;

        [SerializeField]
        Color _cooldownReadyColor = Color.white;

        [SerializeField]
        Color _canUseRolor = Color.lightGreen;

        [SerializeField]
        float _afterEnableCooldown = 0.75f;

        float _currentCheckTime = 0f;

        float _currentCooldownTime = 0f;

        bool _wasOnCooldown = true;
        bool _wasAbleToUse = false;


        bool active = true;
    
        void Start()
        {
            if(_player == null)
            {
                Debug.LogError("Player reference not set in RopeAvailabilityChecker", this);
            }

            if(_ropeIcon == null)
            {
                Debug.LogError("Rope icon reference not set in RopeAvailabilityChecker", this);
            }

            _player.OnSwingInvoked.AddListener(() => 
                {
                    _ropeIcon.OnDeselected();
                    _currentCooldownTime = 0f;
                });
        }

        void Update()
        {
            _currentCheckTime += Time.deltaTime;
            _currentCooldownTime = Mathf.Max(0f, _currentCooldownTime - Time.deltaTime);

            if(_currentCheckTime >= _checkInterval && _currentCooldownTime <= 0f)
            {
                _currentCheckTime = 0f;
                CheckRope();
            }
        }

        void CheckRope()
        {
            bool onCooldown = _player.IsSwingOnCooldown();
            bool canUse = _player.AimingAtSwingable();

            if(onCooldown)
            {
                if(!_wasOnCooldown)
                {
                    _currentCooldownTime = 0;
                    _ropeIcon.OnDeselected();
                }
            }
            else if(canUse)
            {
                if(!_wasAbleToUse || _wasOnCooldown)
                {
                    _currentCooldownTime = _afterEnableCooldown;
                    _ropeIcon.OnSelected();
                }
            }      
            else
            {
                if(_wasAbleToUse || _wasOnCooldown)
                {
                    _currentCooldownTime = 0;
                    _ropeIcon.OnCooldownReady();
                }
            }

            _wasOnCooldown = onCooldown;
            _wasAbleToUse = canUse;
        }
    }
}
