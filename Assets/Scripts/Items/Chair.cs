using System;
using System.Collections;
using MonkeyBusiness.Combat.Health;
using UnityEngine;
using MonkeyBusiness.Enemies.Navigation;
using MonkeyBusiness.Misc;
using Sirenix.Utilities;

namespace MonkeyBusiness.Items
{
    /// <summary>
    /// Throwable item that can be used multiple times before breaking. Hinders enemies hit and deals damage
    /// 
    /// TODO impement enemy knockback when hit
    /// </summary>
    [RequireComponent(typeof(Item))]
    public class Chair : MonoBehaviour
    {
        private Item _item;

/// <summary>
/// Damage dealt to an enemy on thrown impact. Only the enemy directly hit takes damage
/// </summary>
        [SerializeField]
        private int _impactDamage = 25;


        /// <summary>
        /// Number of times the item can be thrown before breaking. Each throw removes 1 durability
        /// </summary>
        [SerializeField]
        private int _durability = 4;

        [SerializeField]
        private float _knockbackForce = 10f;
        [SerializeField]
        private float _knockbackDuration = 0.5f; 

        /// <summary>
        /// Radius of the knockback effect. If an enemy is hit, all enemies in that radius will be knocked back
        /// </summary>
        [SerializeField]
        private float _knockbackEffectRadius = 3.0f;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _item = GetComponent<Item>();

            // reset rotation on throw
            _item.OnThrow.AddListener(()=>{transform.rotation = Quaternion.identity;});
            _item.OnDrop.AddListener(()=>{transform.rotation = Quaternion.identity;});

            _item.OnThrownCollision.AddListener(HandleCollision);
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void HandleCollision(GameObject other)
        {
            if(other.CompareTag("Enemy"))
            {
                // apply knockback to all enemies in radius
                foreach(Collider collider in Physics.OverlapSphere(other.transform.position, 3.0f)){

                    if (collider.gameObject.CompareTag("Enemy"))
                    {
                        KnockbackController knockbackController = collider.gameObject.GetComponentInParent<KnockbackController>();

                        if(knockbackController != null)
                        {
                            var knockbackVector = GetComponentInParent<Rigidbody>().linearVelocity.normalized * _knockbackForce;
                            knockbackVector.y = Math.Abs(knockbackVector.y);
                            // Debug.Log(knockbackVector);
                            knockbackController.Knockback(knockbackVector, _knockbackDuration);
                        }
                    }
                }

                // deal damage to directly hit enemy
                other.GetComponentInParent<HealthController>().TakeDamage(_impactDamage);

                /* if(other.TryGetComponent(out EnemyFollowController enemyFollowController))
                {
                    // TODO implement knockback to enemy controller
                    enemyFollowController.Slowdown(0.25f, 0.5f);
                } */

                /* if(other.TryGetComponent(out KnockbackController knockbackController))
                {
                    var knockbackVector = GetComponentInParent<Rigidbody>().linearVelocity.normalized * _knockbackForce;
                    knockbackVector.y = Math.Abs(knockbackVector.y);
                    Debug.Log(knockbackVector);
                    knockbackController.Knockback(knockbackVector, _knockbackDuration);
                } */
            }

            // Lower durability
            if(--_durability == 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
