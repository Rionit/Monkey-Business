using UnityEngine;

namespace MonkeyBusiness.Items
{
    public class Item : MonoBehaviour
    {

        private Rigidbody _rigidbody;
        public bool isBeingHeld = false;

        [SerializeField]
        private float throwForce = 60.0f;

        [SerializeField]
        private int durability = 100;

        private bool isBeingThrown = false;

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

        public void PickUp(Transform parent)
        {
            transform.parent = parent;
            transform.position = parent.position;
            _rigidbody.isKinematic = true;
            _rigidbody.detectCollisions = false;
            isBeingHeld = true;
        }

        public void Drop()
        {
            transform.parent = null;
            _rigidbody.isKinematic = false;
            _rigidbody.detectCollisions = true;
            isBeingHeld = false;
        }

        public void Throw(Vector3 position, Vector3 direction)
        {
            // ignore collision with thrower until item hits something else
            Transform thrower = transform.root;
            
            transform.position = position;
            Drop();
            _rigidbody.AddForce(direction * throwForce, ForceMode.Impulse);
            isBeingThrown = true;

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

        // CARRYOVER METHOD FROM LAST ITEM ITERATION
        // TODO replace with proper projectile behavior
        void OnCollisionEnter(Collision collision)
        {
            if (!isBeingThrown)
            {
                return;            
            }
            
            Debug.Log(collision.gameObject.name);

            if (collision.gameObject.CompareTag("Enemy"))
            {
                // TODO
                //Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                //enemy.TakeDamage(50);
            }
            
            // Prevent dealing damage multiple times per throw
            isBeingThrown = false;

            // Re-enable collision with whoever threw this item
            Physics.IgnoreCollision(ignoreCollision, GetComponent<Collider>(), false);

            durability -= 34;
            if(durability <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}

