using UnityEngine;

public class BillboardY : MonoBehaviour
{
    void LateUpdate()
    {
        Vector3 direction = Camera.main.transform.position - transform.position;
        direction.y = 0;

        transform.rotation = Quaternion.LookRotation(direction);
    }
}