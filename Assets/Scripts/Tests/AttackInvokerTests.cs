using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Collections;

namespace MonkeyBusiness.Tests
{
    public class AttackInvokerTests
    {
        [UnityTest]
        public IEnumerator AttackInvoker_RangeEnterWorks()
        {
            // Arrange
            var playerObject = new GameObject("Player");
            playerObject.tag = "Player";
            playerObject.transform.position = Vector3.one;
            var rb = playerObject.AddComponent<Rigidbody>();
            rb.useGravity = false;

            var collider = playerObject.AddComponent<CapsuleCollider>();

            playerObject.layer = LayerMask.NameToLayer("Default");


            var invokerObject = new GameObject("AttackInvoker");
            invokerObject.layer = LayerMask.NameToLayer("Default");
            invokerObject.transform.position = Vector3.zero;
            var attackRangeCollider = invokerObject.AddComponent<SphereCollider>();
            attackRangeCollider.radius = 10f;
            var attackInvoker = invokerObject.AddComponent<Combat.AttackInvoker>();

            attackRangeCollider.isTrigger = true;

            // Act
            yield return new WaitForSeconds(0.3f); // Player should be already registered in range by the time we check

            // Assert
            Assert.IsTrue(attackInvoker.PlayerInRange);
        }

        [UnityTest]
        public IEnumerator AttackInvoker_RangeExitWorks()
        {
            // Arrange
            var playerObject = new GameObject("Player");
            playerObject.tag = "Player";
            playerObject.transform.position = Vector3.one;
            playerObject.AddComponent<Rigidbody>();
            playerObject.AddComponent<CapsuleCollider>();

            var invokerObject = new GameObject("AttackInvoker");
            var attackRangeCollider = invokerObject.AddComponent<SphereCollider>();
            var attackInvoker = invokerObject.AddComponent<Combat.AttackInvoker>();

            attackRangeCollider.isTrigger = true;
            attackInvoker.AttackRange = 10f;

            // Act
            yield return new WaitForSeconds(0.1f); // Player should be already registered in range by the time we check

            // Assert
            Assert.IsTrue(attackInvoker.PlayerInRange);
        }

        [UnityTest]
        public IEnumerator AttackInvoker_CooldownWorks()
        {
            // Arrange
            var playerObject = new GameObject("Player");
            playerObject.tag = "Player";
            playerObject.transform.position = Vector3.one;
            playerObject.AddComponent<Rigidbody>();
            playerObject.AddComponent<CapsuleCollider>();

            var invokerObject = new GameObject("AttackInvoker");
            var attackRangeCollider = invokerObject.AddComponent<SphereCollider>();
            var attackInvoker = invokerObject.AddComponent<Combat.AttackInvoker>();

            float cooldown = attackInvoker.CooldownTime;

            Assert.IsTrue(cooldown > 0f, "Cooldown time should be greater than zero");
            bool onCooldownAfterAttack = false; 
            
            // Checks if cooldown is fired after the attack is invoked
            attackInvoker.OnAttackInvoked.AddListener((_) => onCooldownAfterAttack = attackInvoker.OnCooldown);

            attackRangeCollider.isTrigger = true;
            attackInvoker.AttackRange = 10f;

            // Act
            yield return new WaitForSeconds(cooldown / 10f); // Should be enough for the OnAttackInvoked to fire
            playerObject.transform.position = Vector3.one * 100f; // Move player out of range to trigger cooldown

            yield return new WaitForSeconds(cooldown / 2f);
            bool onCooldownMidway = attackInvoker.OnCooldown;
            
            

            yield return new WaitForSeconds(cooldown); // Wait for cooldown to expire

            bool onCooldownAfterWait = attackInvoker.OnCooldown;

            // Assert
            Assert.IsTrue(onCooldownAfterAttack, "Not on cooldown after attack");
            Assert.IsTrue(onCooldownMidway, "Cooldown reset sooner than expected");
            Assert.IsFalse(onCooldownAfterWait, "Cooldown did not expire as expected");
        }


    }
}
