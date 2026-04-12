using System;
using UnityEngine;
using UnityEngine.UI;

namespace MonkeyBusiness.UI
{
    public class HealthBarController : MonoBehaviour
    {
        
        [SerializeField] private Image healthBar;
        
        [Range(0f, 1f)]
        [SerializeField] private float value = 0.5f;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnValidate()
        {
            if (healthBar != null)
            {
                healthBar.rectTransform.sizeDelta = new Vector2(GetFillSize(value), healthBar.rectTransform.sizeDelta.y);
            }
        }

        private float GetFillSize(float value)
        {
            return 50f + value * (GetComponent<RectTransform>().sizeDelta.x - 50f);
        }
    }
}
