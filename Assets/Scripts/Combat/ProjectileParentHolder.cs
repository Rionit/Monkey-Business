using UnityEngine;

namespace MonkeyBusiness.Combat
{
    /// <summary>
    /// A singleton component that holds a reference to the scene's projectile parent
    /// </summary>
    public class ProjectileParentHolder : MonoBehaviour
    {
        public static ProjectileParentHolder Instance { get; private set; }

        [field:SerializeField]
        public GameObject Object {get; private set; }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of ProjectileParentHolder detected! Replacing the old one.");

            }
            Instance = this;
            if(Object == null)
            {
                Object = gameObject; // Default to self if not assigned 
            }
        }
    }
}
