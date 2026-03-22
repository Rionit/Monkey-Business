using UnityEngine;

public class Item : MonoBehaviour
{

    private Rigidbody _rigidbody;
    public bool isBeingHeld = false;

    [SerializeField]
    private float throwForce = 60.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void PickUp(Transform parent)
    {
        transform.parent = parent;
        transform.position = parent.position;
        _rigidbody.isKinematic = true;
        _rigidbody.detectCollisions = false;
        isBeingHeld = true;
    }

    public void Drop()
    {
        transform.parent = null;
        _rigidbody.isKinematic = false;
        _rigidbody.detectCollisions = true;
        isBeingHeld = false;
    }

    public void Throw(Vector3 direction)
    {
        Drop();
        _rigidbody.AddForce(direction * throwForce, ForceMode.Impulse);
    }
}
