using Unity.VisualScripting;
using UnityEngine;

public class BillboardController : MonoBehaviour
{
    /// <summary>
    /// Billboard to rotate towards the camera.
    /// </summary>
    [SerializeField]
    [Tooltip("Billboard to rotate towards the camera")]
    RectTransform _billboardTransform;

    // Update is called once per frame
    void Update()
    {
        _billboardTransform.LookAt(Camera.main.transform);
    }
}
