using System.Collections.Generic;
using MonkeyBusiness.Items;
using MonkeyBusiness.Weapons;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MonkeyBusiness
{
    /// <summary>
    /// Manages the player's inventory, switching weapons, picking up items off the ground, and shooting.
    /// </summary>
    public class EquipmentManager : MonoBehaviour
    {
        InputAction _scrollWheel;

        private InputAction[] _itemInputActions = new InputAction[9];

        private InputAction _interactAction;
        private InputAction _attackAction;

        /// <summary>
        /// Currently held item
        /// </summary>
        private Item _heldItem;

        /// <summary>
        /// The transform where a picked up item will be held.
        /// </summary>
        [SerializeField]
        private Transform itemAttachPoint;

        /// <summary>
        /// Drag the 1st person camera component here
        /// </summary>
        [SerializeField]
        private Transform _cameraTransform;

        /// <summary>
        /// Maximum distance at which the player can interact with the environment
        /// </summary>
        [SerializeField]
        private float _maxPickupDistance = 1.5f;

        /// <summary>
        /// Items are pushed forward slightly when thrown to prevent self-collision
        /// </summary>
        [SerializeField]
        private float _throwPush = 0.5f; // 

        // We can hold 9 items 
        private const int ITEM_CAPACITY = 9;

        /// <summary>
        /// List of items in inventory
        /// </summary>
        private List<IEquippable> _items = new(ITEM_CAPACITY);


        [ShowInInspector]
        [ReadOnly]
        private int _currentItemSlot = -1;
        private int _previousItemSlot = -1;

        void Start()
        {
            _scrollWheel = InputSystem.actions.FindAction("ScrollWheel");


            for(int i = 0; i < ITEM_CAPACITY; i++)
            {
                _itemInputActions[i] = InputSystem.actions.FindAction($"Item{i + 1}");
                int copyOfI = i; // we need to assign this to avoid shenanigans on the next line
                _itemInputActions[i].performed += context => {OnItem(copyOfI);};

                Debug.Log(_itemInputActions[i].ToString());
            }

            _interactAction = InputSystem.actions.FindAction("Interact", true);
            _attackAction = InputSystem.actions.FindAction("Attack");

            _interactAction.performed += OnInteract;
            _attackAction.performed += OnAttack;

            _scrollWheel.performed += OnScroll;

            foreach(IEquippable equippable in GetComponentsInChildren<IEquippable>())
            {
                _items.Add(equippable);
                equippable.Unequip();
            }

            if(_items.Count > 0)
            {
                EquipSlot(0);
            }
        }

        /// <summary>
        /// Handle InputSystem scroll wheel event
        /// </summary>
        /// <param name="context"></param>
        private void OnScroll(InputAction.CallbackContext context)
        {
            float scroll = context.ReadValue<Vector2>().y;
            if(scroll != 0)
            {
                // Scroll up for previous, scroll down for next
                EquipNext(scroll > 0);
            }

        }

        /// <summary>
        /// Handle event fired by pressing buttons 1-9.
        /// Checks if we have enough items in inventory and then calls EquipSlot()
        /// </summary>
        /// <param name="itemSlot"> Number pressed </param>
        void OnItem(int itemSlot)
        {
            Debug.Log($"Trying to equip item {itemSlot}");
            if (itemSlot >= _items.Count)
            {
                Debug.Log($"Tried to equip item in slot {itemSlot}, but I only have {_items.Count} items.");
                return;
            }
            
            EquipSlot(itemSlot);
        }
        
        /// <summary>
        /// Equips the item in the provided slot. If we're holding an item already, unequip/drop it
        /// </summary>
        /// <param name="itemSlot">Position of item in _items</param>
        void EquipSlot(int itemSlot)
        {
            if(itemSlot == _currentItemSlot)
            {
                return;
            }

            var item = _items[itemSlot];

            if(item == null)
            {
                Debug.LogError($"Tried to equip item at slot {itemSlot}, but slot is null");
                return;
            }

            // if we're holding an item, drop it
            DropHeldItem();

            // uneqip previous item
            UnequipCurrentItem();
            
            item.Equip();

            _currentItemSlot = itemSlot;
        }

        /// <summary>
        /// Unequips the currently held item
        /// </summary>
        void UnequipCurrentItem()
        {
            // Unequip previous item
            if(_currentItemSlot == -1)
            {
               return;
            } 
            _items[_currentItemSlot].Unequip();
            _previousItemSlot = _currentItemSlot;
            _currentItemSlot = -1;
        }

        /// <summary>
        /// Equips the next item in inventory
        /// </summary>
        /// <param name="previous">instead equip previous item if true</param>
        void EquipNext(bool previous)
        {
            int currentItemCount = _items.Count;

            if(currentItemCount == 0)
            {
                Debug.Log($"Can't cycle items with empty inventory");
                return;
            }

            int newItemSlot = _currentItemSlot;

            if (previous)
            {
                newItemSlot--;
                if(newItemSlot < 0)
                {
                    newItemSlot = currentItemCount - 1;
                }
            }
            else
            {
                newItemSlot = (newItemSlot + 1) % currentItemCount;
            }
            EquipSlot(newItemSlot);
        }

        /// <summary>
        /// Handle interaction button press
        /// </summary>
        /// <param name="context"></param>
        void OnInteract(InputAction.CallbackContext context)
        {
            Debug.Log("OnInteract");
            if (_heldItem)
            {
                DropHeldItem();
            }
            else
            {
                // Raycast in front of the player
                if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, _maxPickupDistance))
                {
                    GameObject gameObject = hit.transform.gameObject;
                    Debug.Log(gameObject.name);
                    // Check if we hit an item
                    if (gameObject.CompareTag("Item"))
                    {
                        Item item = gameObject.GetComponent<Item>();
                        // Pick the item up
                        item.PickUp(itemAttachPoint);
                        _heldItem = item;

                        //unequip current weapon
                        UnequipCurrentItem();
                    }
                }
            }
        }

        ///<summary>
        /// Handle left click pressed 
        ///</summary>        
        void OnAttack(InputAction.CallbackContext context)
        {
            if (_heldItem)
            {
                ThrowHeldItem();
            }
            else
            {
                _items[_currentItemSlot].Use();
            }
        }

        /// <summary>
        /// Throws the current item if one is being held
        /// </summary>
        public void ThrowHeldItem()
        {
            if(_heldItem == null)
            {
                return;
            }

            _heldItem.Throw(_cameraTransform.position + (_throwPush * _cameraTransform.forward), _cameraTransform.forward);
            _heldItem = null;

            //re-equip previous item
            EquipSlot(_previousItemSlot);
        }

        /// <summary>
        /// Drops the current item if one is being held
        /// </summary>
        public void DropHeldItem()
        {
            if(_heldItem == null)
            {
                return;
            }

            _heldItem.Drop();
            _heldItem = null;
            
            //re-equip last item
            EquipSlot(_previousItemSlot);
        }

        /// <summary>
        /// Returns current equipped item, or null if none is being held
        /// </summary>
        /// <returns> Current equipped item or null </returns>
        public IEquippable GetEquippedWeapon()
        {
            return _currentItemSlot == -1 ? null : _items[_currentItemSlot];
        }
    }
}
