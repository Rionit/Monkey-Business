using UnityEngine;
using Sirenix.OdinInspector;

public struct CameraInput
{
    public Vector2 Look;
}

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float sensitivity = 0.1f;
    private Vector3 _eulerAngles;

    [SerializeField]
    [MinMaxSlider(-90f, 90f)]
    [Tooltip("Limits for the camera's pitch (looking up and down), in degrees.")]
    private Vector2 _pitchLimits = new Vector2(-50f, 50f);

    public void Initialize(Transform target)
    {
        transform.position = target.position;
        transform.eulerAngles = _eulerAngles = target.eulerAngles;
    }

    public void UpdateRotation(CameraInput input)
    {
        
        _eulerAngles += new Vector3(-input.Look.y, input.Look.x) * sensitivity;
        _eulerAngles.x = Mathf.Clamp(_eulerAngles.x, _pitchLimits.x, _pitchLimits.y);
        transform.eulerAngles = _eulerAngles;
    }

    public void UpdatePosition(Transform target)
    {
        transform.position = target.position;
    }
}
