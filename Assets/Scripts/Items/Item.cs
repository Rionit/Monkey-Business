using UnityEngine;
using UnityEngine.Events;

namespace MonkeyBusiness.Items
{
    [RequireComponent(typeof(Rigidbody))]
    /// <summary>
    /// An item that can be picked up from the environment, dropped, or thrown.
    /// </summary>
    public class Item : MonoBehaviour
    {

        private Rigidbody _rigidbody;
        public bool isBeingHeld = false;

        /// <summary>
        /// How much force the item is thrown with
        /// </summary>
        [SerializeField]
        private float throwForce = 60.0f;
        public bool IsBeingThrown = false;

        public UnityEvent OnPickup;

        /// <summary>
        /// This collider will be ignored. Helper variable to prevent collision with player immediately after throwing
        /// </summary>
        public Collider ignoreCollision = null;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
        }

        // Update is called once per frame
        void Update()
        {
        }

        /// <summary>
        /// Called when this item is picked up
        /// </summary>
        /// <param name="parent"> The transform where this item will be carried </param>
        public void PickUp(Transform parent)
        {
            transform.parent = parent;
            transform.position = parent.position;
            _rigidbody.isKinematic = true;
            _rigidbody.detectCollisions = false;
            isBeingHeld = true;

            OnPickup.Invoke();
        }

        /// <summary>
        /// Drop the item on the ground. Lose current parent and enable physics simulation
        /// </summary>
        public void Drop()
        {
            transform.parent = null;
            _rigidbody.isKinematic = false;
            _rigidbody.detectCollisions = true;
            isBeingHeld = false;
        }

        /// <summary>
        /// Throws this item
        /// </summary>
        /// <param name="position">Throw starting position</param>
        /// <param name="direction">Throw direction</param>
        public void Throw(Vector3 position, Vector3 direction)
        {
            // ignore collision with thrower until item hits something else
            Transform thrower = transform.root;
            
            transform.position = position;
            Drop();
            _rigidbody.AddForce(direction * throwForce, ForceMode.Impulse);
            IsBeingThrown = true;

            // TODO replace with logic that works for parents with multiple colliders
            ignoreCollision = thrower.GetComponentInChildren<Collider>();
            Physics.IgnoreCollision(ignoreCollision, GetComponent<Collider>());

        }

        public void LateUpdate()
        {
            // Apply gravity manually
            if (!isBeingHeld)
            {
                _rigidbody.AddForce(Physics.gravity * _rigidbody.mass * _rigidbody.mass, ForceMode.Force);
            }
        }
    }
}

