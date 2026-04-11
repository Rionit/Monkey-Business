using UnityEngine;
using Sirenix.OdinInspector;

namespace MonkeyBusiness.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        
        [Tooltip("If set to <color=green>true</color>, projectiles can be parented under a custom object instead of the default scene root.")]
        bool _customProjectileParent = false;

        [ShowInInspector]
        [Tooltip("Parent object for all projectiles in the scene. Automatically created if not assigned.")]
        public GameObject ProjectileParent { get; private set; }
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of GameManager detected! Replacing the old one.");
            }
            Instance = this;

            if(!_customProjectileParent || ProjectileParent == null)
            {
                ProjectileParent = gameObject; // Default to self if not using custom parent
            }
        }
    }
}