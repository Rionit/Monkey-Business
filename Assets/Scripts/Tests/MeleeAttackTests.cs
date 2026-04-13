using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using NUnit.Framework;
using MonkeyBusiness.Combat.Attack;
using MonkeyBusiness.Combat.Health;

namespace MonkeyBusiness.Tests
{
    public class MeleeAttackTests
    {
        [UnityTest]
        public IEnumerator MeleeAttack_DealsDamage()
        {
            // Arrange
            
            // Player object
            var playerObject = new GameObject("Player");
            playerObject.tag = "Player";
            playerObject.transform.position = Vector3.one;
            playerObject.layer = LayerMask.NameToLayer("Default");

            var rb = playerObject.AddComponent<Rigidbody>();
            rb.useGravity = false;

            var collider = playerObject.AddComponent<CapsuleCollider>();
            var playerHealth = playerObject.AddComponent<HealthController>();

            // Attacker object
            var attackerObject = new GameObject("Attacker");
            attackerObject.layer = LayerMask.NameToLayer("Default");
            var attackRangeCollider = attackerObject.AddComponent<SphereCollider>();
            attackRangeCollider.radius = 10f;
            attackRangeCollider.isTrigger = true;
            var attackInvoker = attackerObject.AddComponent<AttackInvoker>();
            var meleeAttack = attackerObject.AddComponent<MeleeAttackController>();

            // Act
            yield return new WaitForSeconds(meleeAttack.ChargeTime + 0.1f);
            Assert.IsFalse(playerHealth.CurrentHealth == playerHealth.MaxHealth, "Player didn't take any damage");
            Assert.IsTrue(playerHealth.CurrentHealth == playerHealth.MaxHealth - meleeAttack.Damage, "Player didn't take enough damage");
        }

        [UnityTest]
        public IEnumerator MeleeAttack_NoDamageOnPlayerDodge()
        {
            // Arrange
            
            // Player object
            var playerObject = new GameObject("Player");
            playerObject.tag = "Player";
            playerObject.transform.position = Vector3.one;
            playerObject.layer = LayerMask.NameToLayer("Default");

            var rb = playerObject.AddComponent<Rigidbody>();
            rb.useGravity = false;

            var collider = playerObject.AddComponent<CapsuleCollider>();
            var playerHealth = playerObject.AddComponent<HealthController>();

            // Attacker object
            var attackerObject = new GameObject("Attacker");
            attackerObject.layer = LayerMask.NameToLayer("Default");
            var attackRangeCollider = attackerObject.AddComponent<SphereCollider>();
            attackRangeCollider.radius = 10f;
            attackRangeCollider.isTrigger = true;
            var attackInvoker = attackerObject.AddComponent<AttackInvoker>();
            var meleeAttack = attackerObject.AddComponent<MeleeAttackController>();

            bool attacking = false;
            attackInvoker.OnAttackInvoked.AddListener((_) => attacking = true);

            // Act
            yield return new WaitUntil(() => attacking);
            playerObject.transform.position = Vector3.one * 100f; // Player dodges the range

            yield return new WaitForSeconds(meleeAttack.ChargeTime);
            Assert.IsTrue(playerHealth.CurrentHealth == playerHealth.MaxHealth, "Player took damage even though it dodged the attack");
        }
    }
}
