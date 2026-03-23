using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterItemManager : MonoBehaviour
{
    [SerializeField]
    private Transform itemAttachPoint;
    [SerializeField]
    private Transform cameraTransform;

    private InputAction interactAction;
    private InputAction attackAction;

    private Item heldItem = null;

    [SerializeField]
    private float maxPickupDistance = 1.5f;

    [SerializeField]
    private float throwPush = 0.5f; // Push the item slightly forward before throwing

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactAction = InputSystem.actions.FindAction("Interact", true);
        interactAction.performed += OnInteract;
        attackAction = InputSystem.actions.FindAction("Attack");
        attackAction.performed += OnAttack;
    }

    void OnEnable()
    {
        if(interactAction != null){
            interactAction.performed += OnInteract;
        }
        if(attackAction != null)
        {
            attackAction.performed += OnAttack;
        }
    }

    void OnDisable()
    {
        if(interactAction != null)
        {
            interactAction.performed -= OnInteract;
        }
        if(attackAction != null)
        {
            attackAction.performed -= OnAttack;
        }
    }

    // Update is called once per frame
    void Update()
    {   
    }

    void OnInteract(InputAction.CallbackContext context)
    {
        if (heldItem)
        {
            // Drop held item if there is one
            heldItem.Drop();
            heldItem = null;
        }
        else
        {
            // Raycast in front of the player
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, maxPickupDistance))
            {
                GameObject gameObject = hit.transform.gameObject;
                // Check if we hit an item
                if (gameObject.CompareTag("Item"))
                {
                    Item item = gameObject.GetComponent<Item>();
                    // Pick the item up
                    item.PickUp(itemAttachPoint);
                    heldItem = item;
                }
            }
        }
    }

    void OnAttack(InputAction.CallbackContext context)
    {
        if (heldItem)
        {
            heldItem.Throw(cameraTransform.position + (throwPush * cameraTransform.forward), cameraTransform.forward);
            heldItem = null;
        }
    }
}
