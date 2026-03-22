using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    [SerializeField] private List<GunController> guns;

    [SerializeField] Toggle highGunRateToggle;
    [SerializeField] Toggle lowGunRateToggle;
    
    [SerializeField] List<TMP_Text> gunAmmoTexts;


    /// <summary>
    /// Currently chosen gun.
    /// </summary>
    private GunController _currentGun;

    /// <summary>
    /// Index of the currently chosen gun in the guns list.
    /// </summary>
    private int _currentGunIndex = 0;

    private PlayerInputActions _inputActions;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        _inputActions = new PlayerInputActions();
        _inputActions.Enable(); 
        
        playerCharacter.Initialize();
        playerCamera.Initialize(playerCharacter.GetCameraTarget());
        
        cameraSpring.Initialize();
        cameraLean.Initialize();
        
        stanceVignette.Initialize(volume.profile);

        _currentGun = guns[_currentGunIndex];
        _currentGun.gameObject.SetActive(true);

        for(int i = 0; i < guns.Count; i++)
        {
            var index = i;
            var gunAmmoText = gunAmmoTexts[i];
            gunAmmoText.text = $"{guns[index].MaxAmmo} / {guns[index].MaxAmmo}";

            guns[index].OnShot.AddListener((currentAmmo) => ChangeText(index));
        }
    }
    
    void ChangeText(int i)
    {
        var gunAmmoText = gunAmmoTexts[i];
        gunAmmoText.text = $"{guns[i].CurrentAmmo} / {guns[i].MaxAmmo}";
    }

    public void ReloadWeapons(float percent)
    {
        foreach(var gun in guns)
        {
            int ammoToReload = Mathf.RoundToInt(gun.MaxAmmo * (percent / 100f));
            gun.Reload(ammoToReload);
        }
    }


    
    private void OnDestroy()
    {
        _inputActions.Dispose();
    }

    // Update is called once per frame
    void Update()
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
    
        if(input.Attack.IsPressed())
        {
            _currentGun.Fire();
        }
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
        if(input.SwitchWeapon.WasPressedThisFrame())
        {
            _currentGun.gameObject.SetActive(false);
            _currentGunIndex = (_currentGunIndex + 1 + guns.Count) % guns.Count;
            _currentGun = guns[_currentGunIndex];
            _currentGun.gameObject.SetActive(true);
        }

        if(input.ResetLevel.WasPressedThisFrame())
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        var wepPick = input.PickWeapon.ReadValue<float>();
        if(wepPick > 0f)
        {
            int index = (int)wepPick - 1;

            var newGun = guns[index];
            if(newGun != _currentGun)
            {
                _currentGun.gameObject.SetActive(false);
                _currentGun = newGun;
                _currentGun.gameObject.SetActive(true);
            } 
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
