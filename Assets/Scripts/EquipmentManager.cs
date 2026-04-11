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
        }



        // Update is called once per frame
        void Update()
        {
        
        }

    }
}
