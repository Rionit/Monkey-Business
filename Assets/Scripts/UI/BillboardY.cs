using UnityEngine;

public class BillboardY : MonoBehaviour
{
    [SerializeField]
    bool _inverted = false;

    void LateUpdate()
    {
        Vector3 direction = transform.position - Camera.main.transform.position;
        direction.y = 0;

        if (_inverted)
        {
            direction = -direction;
        }

        transform.rotation = Quaternion.LookRotation(direction);
    }
}