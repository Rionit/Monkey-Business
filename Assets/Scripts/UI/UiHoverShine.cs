using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHoverShineReset : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Material runtimeMat;
    private Image img;

    private bool hovering;
    private float shineTime;

    private Vector3 originalScale;
    public float hoverScale = 1.05f;
    public float scaleSpeed = 10f;

    void Awake()
    {
        img = GetComponent<Image>();

        // IMPORTANT: unique material instance
        runtimeMat = Instantiate(img.material);
        img.material = runtimeMat;

        runtimeMat.SetFloat("_Hover", 0);
        runtimeMat.SetFloat("_ShineTime", 0);

        originalScale = transform.localScale;
    }

    void Update()
    {
        // shine animation
        if (hovering)
        {
            shineTime += Time.deltaTime;
            runtimeMat.SetFloat("_ShineTime", shineTime);
        }

        // scale animation (smooth)
        Vector3 target = hovering ? originalScale * hoverScale : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, target, Time.deltaTime * scaleSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;

        shineTime = 0f;

        runtimeMat.SetFloat("_Hover", 1);
        runtimeMat.SetFloat("_ShineTime", 0);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;

        runtimeMat.SetFloat("_Hover", 0);
    }
}