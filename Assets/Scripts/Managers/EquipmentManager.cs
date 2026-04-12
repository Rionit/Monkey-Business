using System.Collections.Generic;
using MonkeyBusiness.Weapons;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MonkeyBusiness
{
    public class EquipmentManager : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created

        InputAction _scrollWheel;
        InputAction _inputItem1;
        InputAction _inputItem2;
        InputAction _inputItem3;
        InputAction _inputItem4;
        InputAction _inputItem5;
        InputAction _inputItem6;
        InputAction _inputItem7;
        InputAction _inputItem8;
        InputAction _inputItem9;

        private const int ITEM_CAPACITY = 9;

        [SerializeField]
        private List<GameObject> _startingItems = new(ITEM_CAPACITY);

        private List<IEquippable> _items = new(ITEM_CAPACITY);


        [ShowInInspector]
        [ReadOnly]
        private int _curentItemSlot = -1;

        void Start()
        {
            _scrollWheel = InputSystem.actions.FindAction("ScrollWheel");
            _inputItem1 = InputSystem.actions.FindAction("Item1");
            _inputItem2 = InputSystem.actions.FindAction("Item2");
            _inputItem3 = InputSystem.actions.FindAction("Item3");
            _inputItem4 = InputSystem.actions.FindAction("Item4");
            _inputItem5 = InputSystem.actions.FindAction("Item5");
            _inputItem6 = InputSystem.actions.FindAction("Item6");
            _inputItem7 = InputSystem.actions.FindAction("Item7");
            _inputItem8 = InputSystem.actions.FindAction("Item8");
            _inputItem9 = InputSystem.actions.FindAction("Item9");

            _inputItem1.performed += context => {OnItem(0);};
            _inputItem2.performed += context => {OnItem(1);};
            _inputItem3.performed += context => {OnItem(2);};
            _inputItem4.performed += context => {OnItem(3);};
            _inputItem5.performed += context => {OnItem(4);};
            _inputItem6.performed += context => {OnItem(5);};
            _inputItem7.performed += context => {OnItem(6);};
            _inputItem8.performed += context => {OnItem(7);};
            _inputItem9.performed += context => {OnItem(8);};

            _scrollWheel.performed += OnScroll;

            foreach(GameObject item in _startingItems)
            {
                if(item.TryGetComponent<IEquippable>(out var equippable))
                {
                    _items.Add(equippable);
                    equippable.Unequip();
                }
                else
                {
                    Debug.LogError($"Skipped equipping the player with GameObject {item.name}, as it doesn't have an IEquippable component");
                }
            }

            if(_items.Count > 0)
            {
                EquipSlot(0);
            }
        }

        private void OnScroll(InputAction.CallbackContext context)
        {
            float scroll = context.ReadValue<Vector2>().y;
            if(scroll != 0)
            {
                // Scroll up for previous, scroll down for next
                EquipNext(scroll > 0);
            }

        }

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

        void EquipSlot(int itemSlot)
        {
            if(itemSlot == _curentItemSlot)
            {
                return;
            }

            var item = _items[itemSlot];

            if(item == null)
            {
                Debug.LogError($"Tried to equip item at slot {itemSlot}, but slot is null");
                return;
            }

            
            // Unequip previous item
            if(_curentItemSlot != -1)
            {
                _items[_curentItemSlot].Unequip();
            }
            
            item.Equip();
            _curentItemSlot = itemSlot;
        }

        void EquipNext(bool previous)
        {
            int currentItemCount = _items.Count;

            if(currentItemCount == 0)
            {
                Debug.Log($"Can't cycle items with empty inventory");
                return;
            }

            int newItemSlot = _curentItemSlot;

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

    }
}
