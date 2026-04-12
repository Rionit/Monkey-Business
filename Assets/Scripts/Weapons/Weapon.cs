using UnityEngine;

namespace MonkeyBusiness.Weapons
{
    
    public class Weapon : MonoBehaviour, IEquippable
    {    
        //private Transform[] _transforms = {};

        public void Equip()
        {
            Debug.Log($"Equipped item {gameObject.name}");
            gameObject.SetActive(true);

            // Setting to default now, change later if needed
            //SetChildLayers(0);
        }

        public void Unequip()
        {
            Debug.Log($"Unequipped item {gameObject.name}");
            //SetChildLayers(LayerMask.NameToLayer("UnequippedItem"));
            gameObject.SetActive(false);
        }

        public void Use()
        {
            Debug.Log($"{gameObject.name} says \"Bang!\"");
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        void Awake()
        {
            //_transforms = GetComponentsInChildren<Transform>();
        }

        // Update is called once per frame
        void Update()
        {
        
        }


        /*
        private void SetChildLayers(int layer)
        {
            foreach(Transform t in _transforms)
            {
                t.gameObject.layer = layer;
            }
        }
        */
    }
}
