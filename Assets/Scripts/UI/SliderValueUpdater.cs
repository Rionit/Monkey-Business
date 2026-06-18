using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace MonkeyBusiness.UI
{
    public class SliderValueUpdater : MonoBehaviour
    {
        [SerializeField]
        TMP_Text _valueText;

        [SerializeField]
        int _numDigits = 0;

        public void UpdateValue(float value)
        {
            float multiplier = Mathf.Pow(10, _numDigits);
            value = Mathf.Round(value * multiplier) / multiplier;
            _valueText.text = value.ToString();
        }
    }
}
