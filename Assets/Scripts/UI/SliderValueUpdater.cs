using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace MonkeyBusiness.UI
{
    public class SliderValueUpdater : MonoBehaviour
    {
        [SerializeField]
        TMP_Text _valueText;

        public void UpdateValue(float value)
        {
            _valueText.text = Mathf.RoundToInt(value).ToString();
        }
    }
}
