using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools.Utils;

using MonkeyBusiness.Combat.Health;
using MonkeyBusiness.Combat.Weapons;

namespace MonkeyBusiness.Tests
{
    public class ProjectileTests
    {
        GameObject _projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/Test Projectile.prefab");

        [UnityTest]
        public IEnumerator Projectile_InitializationWorks()
        {
            // Arrange
            var projectileObject = GameObject.Instantiate(_projectilePrefab);
            var projectileController = projectileObject.GetComponent<ProjectileController>();

            string expectedTag = "Player";
            Vector3 expectedDirection = new Vector3(1, 0, 0);

            // Act
            projectileController.Initialize(expectedTag, expectedDirection);

            // Assert
            Assert.AreEqual(expectedTag, projectileController.TargetTag, 
            "Projectile target tag was not set properly. Expected: " + expectedTag + ", Actual: " + projectileController.TargetTag);
            
            var dirEqual = Vector3ComparerWithEqualsOperator.Instance.Equals(expectedDirection, projectileController.Direction);
            Assert.IsTrue(dirEqual, 
            $"Projectile direction was not set properly. Expected: {expectedDirection}, Actual: {projectileController.Direction}");

            yield break;
        }

        [UnityTest]
        public IEnumerator Projectile_HitsTarget()
        {
            // Arrange
            var playerObject = new GameObject("Player");
            playerObject.tag = "Player";
            playerObject.transform.position = Vector3.forward * 5f;
            playerObject.layer = LayerMask.NameToLayer("Default");
            var rb = playerObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            var collider = playerObject.AddComponent<CapsuleCollider>();
            var playerHealth = playerObject.AddComponent<HealthController>();

            var projectileObject = GameObject.Instantiate(_projectilePrefab);
            var projectileController = projectileObject.GetComponent<ProjectileController>();
            projectileController.Initialize("Player", Vector3.forward);

            var distance = Vector3.Distance(playerObject.transform.position, projectileObject.transform.position);

            // Act
            yield return new WaitForSeconds((distance / projectileController.Speed) + 1f); // Waits for the projectile to hit

            // Assert
            Assert.AreEqual(playerHealth.MaxHealth - projectileController.Damage, playerHealth.CurrentHealth,
            $"Player health was not reduced properly. " + 
            $"Expected: {playerHealth.MaxHealth - projectileController.Damage} " +
            $", Actual: {playerHealth.CurrentHealth}");
        }

        [UnityTest]
        public IEnumerator Projectile_DestroysAfterMaxDistance()
        {
            // Arrange
            var projectileObject = GameObject.Instantiate(_projectilePrefab);
            var projectileController = projectileObject.GetComponent<ProjectileController>();
            projectileController.Initialize("Player", Vector3.forward);

            // Act
            yield return new WaitForSeconds((projectileController.MaxFlyDistance / projectileController.Speed) + 0.1f); // Waits for the projectile to reach max distance

            // Assert

            var isDestroyed = projectileObject == null;
            Assert.IsTrue(isDestroyed, "Projectile was not destroyed after reaching max fly distance.");
        }

    }
}