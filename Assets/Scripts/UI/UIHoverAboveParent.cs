using UnityEngine;

namespace MonkeyBusiness.UI
{
    public class UIHoverAboveParent : MonoBehaviour
    {
        [SerializeField]
        private float _YOffset = 3.33f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = transform.parent.position + _YOffset * Vector3.up;
        }
    }
}
