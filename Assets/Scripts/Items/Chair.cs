using UnityEngine;

namespace MonkeyBusiness.Items
{
    [RequireComponent(typeof(Item))]
    public class Chair : MonoBehaviour
    {
        private Item _item;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _item = GetComponent<Item>();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
