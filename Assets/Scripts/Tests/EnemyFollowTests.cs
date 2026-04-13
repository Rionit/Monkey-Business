using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using MonkeyBusiness.Enemies;
using NUnit.Framework;

namespace MonkeyBusiness.Tests
{
    public class EnemyFollowTests
    {
        List<GameObject> _agents;
        List<GameObject> _targets;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            SceneManager.LoadScene("ChokepointTest");
            yield return null;

            _agents = new List<GameObject>(5);
            _targets = new List<GameObject>(5);
            for(int i = 0; i < 5; i++)
            {
                string name = "Agent" + (i + 1).ToString();
                var agent = GameObject.Find(name);
                _agents.Add(agent);

                _targets.Add(GameObject.Find($"Target{i+1}"));
            }
        }

        [UnityTest]
        public IEnumerator EnemyFollow_MovesThroughChokepoint()
        {
            // Arrange
         
            List<GameObject> distanceObjects = new List<GameObject>(5);
            for(int i = 0; i < 5; i++)
            {
                var agent = _agents[i];

                var distanceObject = agent.transform.Find("Capsule");
                distanceObjects.Add(distanceObject.gameObject);
            }

            // Act
            yield return new WaitForSeconds(10f); // Wait for the agents to start moving

            // Assert
            for(int i = 0; i < 5; i++)
            {
                float distance = Vector3.Distance(distanceObjects[i].transform.position, distanceObjects[i].transform.position);
                if (distance > 0.5f)
                {
                    Assert.Fail($"Agent {i+1} did not reach its target. Distance: {distance}");
                }
            }
            Assert.Pass();
        }

        [UnityTest]
        public IEnumerator EnemyFollow_MovesToTarget()
        {
            // Arrange
            for(int i = 1; i < 5; i++)
            {
                _agents[i].SetActive(false);
            }

            var distanceObject = _agents[0].transform.Find("Capsule").gameObject;

            // Act
            yield return new WaitForSeconds(10f); // Wait for the agent to start moving

            // Assert
            float distance = Vector3.Distance(distanceObject.transform.position,
            _agents[0].GetComponent<EnemyFollowController>().ChaseTarget.transform.position);
            Assert.LessOrEqual(distance, 0.5f, $"Agent did not reach the target. Distance: {distance}");
        }

        [UnityTest]
        public IEnumerator EnemyFollow_MaintainsDistance()
        {
            // Arrange
            for(int i = 1; i < 5; i++)
            {
                _agents[i].SetActive(false);
            }

            _agents[0].GetComponent<EnemyFollowController>().TestSetup(5f);

            // Act
            yield return new WaitForSeconds(10f); // Wait for the agent to start moving

            // Assert
            float distance = Vector3.Distance(_agents[0].transform.position,
            _agents[0].GetComponent<EnemyFollowController>().ChaseTarget.transform.position);

            float distanceFromDesired = Mathf.Abs(distance - 5f);
            Assert.LessOrEqual(distanceFromDesired, 0.5f, 
            $"Agent did not maintain the desired distance from the target. Distance from desired: {distanceFromDesired}");

        }


        [UnityTearDown]
        public IEnumerator TearDown()
        {
            //yield break;
            yield return SceneManager.UnloadSceneAsync("ChokepointTest");
        }

    }
}
