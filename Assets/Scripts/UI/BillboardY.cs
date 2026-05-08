using UnityEngine;

public class BillboardY : MonoBehaviour
{
    void LateUpdate()
    {
        Vector3 direction = transform.position - Camera.main.transform.position;
        direction.y = 0;

        transform.rotation = Quaternion.LookRotation(direction);
    }
}