using UnityEngine;

public class Item : MonoBehaviour
{

    private Rigidbody _rigidbody;
    private PlayerInputActions _inputActions;

    public bool isBeingHeld = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _inputActions = new PlayerInputActions();
        _inputActions.Enable();

    }

    // Update is called once per frame
    void Update()
    {
        if (_inputActions.Player.Interact.WasPressedThisFrame())
        {
            if (!isBeingHeld)
            {
                var attachPoint = GameObject.Find("Item Attach Point");
                PickUp(attachPoint.transform);
            }
            else
            {
                Drop();
            }
        }
    }

    void PickUp(Transform parent)
    {
        transform.parent = parent;
        transform.position = parent.position;
        _rigidbody.isKinematic = true;
        _rigidbody.detectCollisions = false;
        isBeingHeld = true;
    }

    void Drop()
    {
        transform.parent = null;
        _rigidbody.isKinematic = false;
        _rigidbody.detectCollisions = true;
        isBeingHeld = false;
    }
}
