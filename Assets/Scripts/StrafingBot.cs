using UnityEngine;

public class StrafingBot : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 2.0f;
    [SerializeField]
    private Vector3 moveDirection = new(1.0f, 0.0f, 0.0f); 
    [SerializeField]
    private float maxDistance = 10.0f;
    private Vector3 startPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        if(Vector3.Distance(transform.position, startPos) > maxDistance)
        {
            transform.position = startPos + maxDistance * moveDirection;
            moveDirection *= -1.0f;
        }
    }
}
