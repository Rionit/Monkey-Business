using UnityEngine;
using UnityEngine.UI;
using MonkeyBusiness.Managers;

namespace MonkeyBusiness.UI
{
    public class BodiesSliderUpdater : MonoBehaviour
    {
        [SerializeField]
        Slider _slider;
    
        void Start()
        {
            if(_slider != null)
            {
                _slider.value = DeadBodiesManager.MaxDeadBodies;
            }
        }
    }
}
