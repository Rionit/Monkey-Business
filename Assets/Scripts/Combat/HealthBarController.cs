using UnityEngine;

public class HealthBarController : MonoBehaviour
{

    /// <summary>
    /// The RectTransform representing the health bar.
    /// </summary>
    RectTransform _healthBar;

    float _maxWidth;


    [SerializeField]
    [Tooltip("Health controller to track health for")]
    HealthController healthController;

    public void Start()
    {
        _healthBar = GetComponent<RectTransform>();
        _maxWidth = _healthBar.sizeDelta.x;

        healthController.OnHealthChanged.AddListener(OnHealthChanged);
    }

    void OnHealthChanged(float newHealth)
    {
        var healthPercent = newHealth / healthController.MaxHealth;
        _healthBar.sizeDelta = new Vector2(_maxWidth * healthPercent, _healthBar.sizeDelta.y);
    }
}
