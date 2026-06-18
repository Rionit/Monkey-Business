using UnityEngine;
using UnityEngine.UI;
using MonkeyBusiness.Camera;

namespace MonkeyBusiness.UI
{
    public class SensitivitySlider : MonoBehaviour
    {

        [SerializeField]
        Slider _slider;


        void Start()
        {
            if(_slider != null)
            {
                _slider.value = PlayerCamera.sensitivityModifier;
                _slider.onValueChanged.Invoke(PlayerCamera.sensitivityModifier);
            }
        }

        public void ResetSensitivity()
        {
            if(_slider != null)
            {
                _slider.value = 1f;
                _slider.onValueChanged.Invoke(1f);
            }
        }

        public void UpdateSensitivity(float value)
        {
            PlayerCamera.sensitivityModifier = value;
        }

        void OnDestroy()
        {
            PlayerPrefs.SetFloat("Sensitivity", PlayerCamera.sensitivityModifier);
        }


    }
}
