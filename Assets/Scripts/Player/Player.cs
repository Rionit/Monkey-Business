using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TextCore.Text;
using Sirenix.OdinInspector;
using MonkeyBusiness.Camera;

namespace MonkeyBusiness.Player
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private PlayerCharacter playerCharacter;
        [SerializeField] private PlayerCamera playerCamera;
        [Space]
        [SerializeField] private CameraSpring cameraSpring;
        [SerializeField] private CameraLean cameraLean;
        [Space]
        [SerializeField] private Volume volume;
        [SerializeField] private StanceVignette stanceVignette;


        [ShowInInspector]
        [BoxGroup("Debug")]
        public bool CanReceiveInput { get; set; } = true;

        private PlayerInputActions _inputActions;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            //if(Target == null) Target = gameObject;
            Cursor.lockState = CursorLockMode.Locked;
            
            _inputActions = new PlayerInputActions();
            _inputActions.Enable(); 
            
            playerCharacter.Initialize();
            playerCamera.Initialize(playerCharacter.GetCameraTarget());
            
            cameraSpring.Initialize();
            cameraLean.Initialize();
            
            stanceVignette.Initialize(volume.profile);
        }

        private void OnDestroy()
        {
            _inputActions.Dispose();
        }

        // Update is called once per frame
        void Update()
        {
            if(CanReceiveInput)
            {
                var input = _inputActions.Player;
                var deltaTime = Time.deltaTime;
                
                // Get camera input and update its rotation
                var cameraInput = new CameraInput { Look = input.Look.ReadValue<Vector2>() };
                playerCamera.UpdateRotation(cameraInput);
                
                // Get character input and update it
                var characterInput = new CharacterInput
                {
                    Rotation    = playerCamera.transform.rotation,
                    Move        = input.Move.ReadValue<Vector2>(),
                    Jump        = input.Jump.WasPressedThisFrame(),
                    JumpSustain = input.Jump.IsPressed(),
                    // Press to toggle crouch, TODO: Maybe add to settings as an option?
                    //Crouch      = input.Crouch.WasPressedThisFrame() ? CrouchInput.Toggle : CrouchInput.None 
                    Crouch = input.Crouch.IsPressed() ? CrouchInput.Crouch : CrouchInput.Uncrouch
                };
                playerCharacter.UpdateInput(characterInput);
                playerCharacter.UpdateBody(deltaTime);
                
                #if UNITY_EDITOR
                if (Keyboard.current.tKey.wasPressedThisFrame)
                {
                    var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        Teleport(hit.point); 
                    }
                }
                #endif
            }
            else
            {
                CharacterInput characterInput = new CharacterInput
                {
                    Rotation    = playerCamera.transform.rotation,
                    Move        = Vector2.zero,
                    Jump        = false,
                    JumpSustain = false,
                };

                playerCharacter.UpdateInput(characterInput);
                playerCharacter.UpdateBody(Time.deltaTime);
            }
        }

        void LateUpdate()
        {
            var deltaTime = Time.deltaTime;
            var cameraTarget = playerCharacter.GetCameraTarget();
            var state = playerCharacter.GetState();
            
            playerCamera.UpdatePosition(playerCharacter.GetCameraTarget());
            cameraSpring.UpdateSpring(deltaTime, cameraTarget.up);
            cameraLean.UpdateLean(deltaTime, state.Stance is Stance.Slide, state.Acceleration,cameraTarget.up);
            
            stanceVignette.UpdateVignette(deltaTime, state.Stance);
        }

        public void Teleport(Vector3 position)
        {
            playerCharacter.SetPosition(position);
        }
    }
}