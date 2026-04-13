using UnityEngine;
using NUnit.Framework;
using MonkeyBusiness.Combat.Health;
using UnityEngine.TestTools;
using System.Collections;

namespace MonkeyBusiness.Tests
{
    public class HealthTests
    {
        [UnityTest]
        public IEnumerator TakeDamage_ReducesHealth()
        {
            // Arrange
            var gameObject = new GameObject();
            var healthController = gameObject.AddComponent<HealthController>();
            var maxHealth = healthController.MaxHealth;
            yield return null; // Start() is called after the first frame, so we wait for it to initialize health

            // Act
            healthController.TakeDamage(30f);
            yield return null;

            // Assert
            Assert.AreEqual(maxHealth - 30f, healthController.CurrentHealth);
        }

        /// <remarks>
        /// Depends on <see cref="HealthController.TakeDamage(float)"/> as well. In case of failure, check it that method works properly. 
        /// </remarks>
        [UnityTest]
        public IEnumerator Heal_IncreasesHealth()
        {
            // Arrange
            var gameObject = new GameObject();
            var healthController = gameObject.AddComponent<HealthController>();
            var maxHealth = healthController.MaxHealth;
            yield return null; // Start() is called after the first frame, so we wait for it to initialize health

            // Act
            healthController.TakeDamage(50f);
            yield return null;
            healthController.Heal(20f);
            yield return null;

            // Assert
            Assert.AreEqual(maxHealth - 30f, healthController.CurrentHealth);
        }

        [UnityTest]
        public IEnumerator TakeDamage_KillsEntity()
        {
            // Arrange
            var gameObject = new GameObject();
            var healthController = gameObject.AddComponent<HealthController>();
            var maxHealth = healthController.MaxHealth;
            yield return null; // Start() is called after the first frame, so we wait for it to initialize health

            bool deathEventInvoked = false;
            healthController.OnDeath.AddListener((obj) => deathEventInvoked = true);

            // Act
            healthController.TakeDamage(maxHealth);
            yield return null;

            // Assert
            Assert.IsTrue(deathEventInvoked);
        }

        [UnityTest]
        public IEnumerator TakeDamage_OverkillWorks()
        {
            // Arrange
            var gameObject = new GameObject();
            var healthController = gameObject.AddComponent<HealthController>();
            var maxHealth = healthController.MaxHealth;
            yield return null; // Start() is called after the first frame, so we wait for it to initialize health

            bool deathEventInvoked = false;
            healthController.OnDeath.AddListener((obj) => deathEventInvoked = true);

            // Act
            healthController.TakeDamage(2 * maxHealth);
            yield return null;

            // Assert
            Assert.IsTrue(deathEventInvoked);
        }

        [UnityTest]
        public IEnumerator Heal_DoesNotOverheal()
        {
            // Arrange
            var gameObject = new GameObject();
            var healthController = gameObject.AddComponent<HealthController>();
            var maxHealth = healthController.MaxHealth;
            yield return null; // Start() is called after the first frame, so we wait for it to initialize health

            // Act
            healthController.Heal(50f);
            yield return null;

            // Assert
            Assert.AreEqual(maxHealth, healthController.CurrentHealth);
        }
    }
}
