using UnityEngine;
using UnityEngine.TestTools;
using UnityEditor;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools.Utils;

namespace MonkeyBusiness.Tests
{
    public class RangedAttackTests
    {
        GameObject _projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/Test Projectile.prefab");

        [UnityTest]
        public IEnumerator RangedAttack_SpawnsProjectile()
        {
            // Arrange
            
            // Player object
            var playerObject = new GameObject("Player");
            playerObject.tag = "Player";
            playerObject.transform.position = Vector3.one * 5f;
            playerObject.layer = LayerMask.NameToLayer("Default");

            var rb = playerObject.AddComponent<Rigidbody>();
            rb.useGravity = false;

            var collider = playerObject.AddComponent<CapsuleCollider>();
            var playerHealth = playerObject.AddComponent<Combat.HealthController>();

            // Attacker object
            var attackerObject = new GameObject("Attacker");
            attackerObject.layer = LayerMask.NameToLayer("Default");
            var attackRangeCollider = attackerObject.AddComponent<SphereCollider>();
            attackRangeCollider.radius = 10f;
            attackRangeCollider.isTrigger = true;
            var attackInvoker = attackerObject.AddComponent<Combat.AttackInvoker>();
            var rangedAttack = attackerObject.AddComponent<Combat.RangedAttackController>();
            var firePos = new GameObject("FirePos");
            firePos.transform.parent = attackerObject.transform;
            firePos.transform.localPosition = Vector3.one * 0.5f;


            rangedAttack.TestSetup(_projectilePrefab, firePos.transform);

            var gameManager = new GameObject("GameManager").AddComponent<Managers.GameManager>();

            bool hasFired = false;
            attackInvoker.OnAttackInvoked.AddListener((_) => hasFired = true);

            Debug.Log("Pre-spawn log");
            // Act
            yield return new WaitUntil(() => hasFired); // Waits until the attack is invoked
            yield return new WaitForSeconds(rangedAttack.ChargeTime + 0.1f); // Waits for the projectile to be spawned


            // Assert

            // Checks if the projectile was spawned 
            var projectile = gameManager.ProjectileParent.GetComponentInChildren<Combat.ProjectileController>();
            Assert.IsNotNull(projectile, "Projectile was not spawned.");
        }

                [UnityTest]
        public IEnumerator RangedAttack_ProjectileSetupProperly()
        {
            // Arrange
            
            // Player object
            var playerObject = new GameObject("Player");
            playerObject.tag = "Player";
            playerObject.transform.position = Vector3.one * 5f;
            playerObject.layer = LayerMask.NameToLayer("Default");

            var rb = playerObject.AddComponent<Rigidbody>();
            rb.useGravity = false;

            var collider = playerObject.AddComponent<CapsuleCollider>();
            var playerHealth = playerObject.AddComponent<Combat.HealthController>();

            // Attacker object
            var attackerObject = new GameObject("Attacker");
            attackerObject.layer = LayerMask.NameToLayer("Default");
            var attackRangeCollider = attackerObject.AddComponent<SphereCollider>();
            attackRangeCollider.radius = 10f;
            attackRangeCollider.isTrigger = true;
            var attackInvoker = attackerObject.AddComponent<Combat.AttackInvoker>();
            var rangedAttack = attackerObject.AddComponent<Combat.RangedAttackController>();
            var firePos = new GameObject("FirePos");
            firePos.transform.parent = attackerObject.transform;
            firePos.transform.localPosition = Vector3.one * 0.5f;

            rangedAttack.TestSetup(_projectilePrefab, firePos.transform);

            var gameManager = new GameObject("GameManager").AddComponent<Managers.GameManager>();

            bool hasFired = false;
            attackInvoker.OnAttackInvoked.AddListener((_) => hasFired = true);

            Vector3 dirToPlayer = (playerObject.transform.position - firePos.transform.position).normalized;

            // Act
            yield return new WaitUntil(() => hasFired); // Waits until the attack is invoked
            yield return new WaitForSeconds(rangedAttack.ChargeTime + 0.1f); // Waits for the projectile to be spawned


            // Assert

            // Checks if the projectile was spawned 
            var projectile = gameManager.ProjectileParent.GetComponentInChildren<Combat.ProjectileController>();
            Assert.AreEqual("Player", projectile.TargetTag, "Projectile target tag was not set properly.");
            
            var dirEqual = Vector3ComparerWithEqualsOperator.Instance.Equals(dirToPlayer, projectile.Direction);
            Assert.IsTrue(dirEqual, $"Projectile direction was not set properly. Expected: {dirToPlayer}, Actual: {projectile.Direction}");
        }
    }
}
