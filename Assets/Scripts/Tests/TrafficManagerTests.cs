using UnityEngine;
using NUnit.Framework;
using MonkeyBusiness.Enemies.Navigation;
using UnityEngine.TestTools;
using System.Collections.Generic;
using System.Linq;
using MonkeyBusiness.Misc;
using UnityEditor.Analytics;
using MonkeyBusiness.Managers;


namespace MonkeyBusiness.Tests
{
    public class TrafficManagerTests
    {   
        GameObject _trafficManagerGO;
        TrafficManager _trafficManager;


        [SetUp]
        public void UnitSetup()
        {
            _trafficManagerGO = new GameObject("TrafficManager");
            _trafficManagerGO.AddComponent<GameManager>();
            _trafficManager = _trafficManagerGO.AddComponent<TrafficManager>();
        }

        [TearDown]
        public void UnitTearDown()
        {
            Object.DestroyImmediate(_trafficManagerGO);
        }

        [Test]
        public void NeighborDistances()
        {
            // Arrange
            var zoneA = new GameObject("ZoneA");
            var zoneB = new GameObject("ZoneB");
            var zoneC = new GameObject("ZoneC");

            zoneA.AddComponent<BoxCollider>();
            zoneB.AddComponent<BoxCollider>();
            zoneC.AddComponent<BoxCollider>();

            var trafficZoneA = zoneA.AddComponent<TrafficZone>();
            var trafficZoneB = zoneB.AddComponent<TrafficZone>();
            var trafficZoneC = zoneC.AddComponent<TrafficZone>();

            foreach(var zone in new TrafficZone[] { trafficZoneA, trafficZoneB, trafficZoneC })
            {
                zone._Size = Vector3.one * 1.2f;
                zone.Keypoint = zone.transform;
            }


            zoneA.transform.position = Vector3.left;
            zoneB.transform.position = Vector3.right;
            zoneC.transform.position = Vector3.forward;

            float[,] expectedDistances = new float[,]
            {
                { 0, 1, 1},
                { Vector3.Distance(trafficZoneA.Keypoint.position, trafficZoneB.Keypoint.position), 0, 1},
                { Vector3.Distance(trafficZoneA.Keypoint.position, trafficZoneC.Keypoint.position), Vector3.Distance(trafficZoneB.Keypoint.position, trafficZoneC.Keypoint.position), 0},
            };

            // Act
            _trafficManager.Connect(trafficZoneA, trafficZoneB);
            _trafficManager.Connect(trafficZoneB, trafficZoneC);
            _trafficManager.Connect(trafficZoneA, trafficZoneC);

            _trafficManager._zones = new TrafficZone[] { trafficZoneA, trafficZoneB, trafficZoneC };
            _trafficManager._distances = new TwoDArray<float>(3, 3); // Initialize the distance matrix
            _trafficManager.CountNeighborDistances();

            // Assert
            bool allDistancesCorrect = true;
            for (int i = 0; i < _trafficManager._distances.Height; i++)
            {
                for(int j = 0; j < _trafficManager._distances.Width; j++)
                {
                    if (_trafficManager._distances[i, j] != expectedDistances[i, j])
                    {
                        allDistancesCorrect = false;
                        break;
                    }
                }
            }
            Assert.IsTrue(allDistancesCorrect, "Neighbor distances aren't calculated correctly.");
        }

        [Test]
        public void FindConnectZones()
        {
            // Arrange
            var zoneA = new GameObject("ZoneA"); // Connected to B
            var zoneB = new GameObject("ZoneB"); // Connected to A and C
            var zoneC = new GameObject("ZoneC"); // Only connected to B
            var zoneD = new GameObject("ZoneD"); // Not connected to others

            zoneA.AddComponent<BoxCollider>();
            zoneB.AddComponent<BoxCollider>();
            zoneC.AddComponent<BoxCollider>();
            zoneD.AddComponent<BoxCollider>();

            var trafficZoneA = zoneA.AddComponent<TrafficZone>();
            var trafficZoneB = zoneB.AddComponent<TrafficZone>();
            var trafficZoneC = zoneC.AddComponent<TrafficZone>();
            var trafficZoneD = zoneD.AddComponent<TrafficZone>();


            foreach(var zone in new TrafficZone[] { trafficZoneA, trafficZoneB, trafficZoneC, trafficZoneD })
            {
                zone._zoneCollider = zone.GetComponent<BoxCollider>();
                zone._Size = Vector3.one * 2.01f;
                zone.Keypoint = zone.transform;
            }

            zoneA.transform.position = Vector3.left;
            zoneB.transform.position = Vector3.right;
            zoneC.transform.position = (Vector3.right * 2f) + Vector3.forward;
            zoneD.transform.position = Vector3.up * 10; // Far away from others
            Physics.SyncTransforms();

            bool intersects = true;

            Debug.Log("Zone A bounds Position: " + trafficZoneA.ZoneBounds.center + " Size: " + trafficZoneA.ZoneBounds.size);
            Debug.Log("Zone B bounds Position: " + trafficZoneB.ZoneBounds.center + " Size: " + trafficZoneB.ZoneBounds.size);
            Debug.Log("Zone C bounds Position: " + trafficZoneC.ZoneBounds.center + " Size: " + trafficZoneC.ZoneBounds.size);
            Debug.Log("Zone D bounds Position: " + trafficZoneD.ZoneBounds.center + " Size: " + trafficZoneD.ZoneBounds.size);

            if(!trafficZoneA.ZoneBounds.Intersects(trafficZoneB.ZoneBounds))
            {
                Debug.LogError("Zone A and B should intersect.");
                intersects = false;
            }
                if(!trafficZoneB.ZoneBounds.Intersects(trafficZoneC.ZoneBounds))
                {
                    Debug.LogError("Zone B and C should intersect.");
                    intersects = false;
                }
            if(trafficZoneA.ZoneBounds.Intersects(trafficZoneC.ZoneBounds))
            {
                Debug.LogError("Zone A and C should not intersect.");
                intersects = false;
            }
            if(trafficZoneA.ZoneBounds.Intersects(trafficZoneD.ZoneBounds))
            {
                Debug.LogError("Zone A and D should not intersect.");
                intersects = false;
            }
            if(trafficZoneB.ZoneBounds.Intersects(trafficZoneD.ZoneBounds))
            {
                Debug.LogError("Zone B and D should not intersect.");
                intersects = false;
            }
            if(trafficZoneC.ZoneBounds.Intersects(trafficZoneD.ZoneBounds))
            {
                Debug.LogError("Zone C and D should not intersect.");
                intersects = false;
            }




            Dictionary<TrafficZone, List<TrafficZone>> expectedNeighbors = new Dictionary<TrafficZone, List<TrafficZone>>()
            {
                { trafficZoneA, new List<TrafficZone>(){ trafficZoneB } },
                { trafficZoneB, new List<TrafficZone>(){ trafficZoneA, trafficZoneC } },
                { trafficZoneC, new List<TrafficZone>(){ trafficZoneB } },
                { trafficZoneD, new List<TrafficZone>() }
            };

            // Act
            _trafficManager.FindAndConnectZones();

            // Assert
            bool hasAllZones = true;
            bool allNeighborsCorrect = true;

            foreach(var zone in expectedNeighbors.Keys)
            {
                if(!_trafficManager._zones.Contains(zone))
                {
                    hasAllZones = false;
                    break;
                }

                var actualNeighbors = zone.Neighbors;
                var expectedNeighborsList = expectedNeighbors[zone];
                if(expectedNeighborsList.Count != actualNeighbors.Count || !expectedNeighborsList.All(en => actualNeighbors.Contains(en)))
                {
                    allNeighborsCorrect = false;
                    break;
                }
            }

            Assert.IsTrue(hasAllZones, "Not all zones were found and added to the manager.");
            Assert.IsTrue(allNeighborsCorrect, "Neighbors aren't connected correctly.");
        }

        [Test]
        public void NonNeighborDistances_AllConnected_SameDistance()
        {
            // Arrange

            // All zones connected with distance 1.0
            float[,] defaultDistances = new float[,]
            {
                { 0, 1, 1, 1, 1},
                { 1, 0, 1, 1, 1},
                { 1, 1, 0, 1, 1},
                { 1, 1, 1, 0, 1},
                { 1, 1, 1, 1, 0},
            };

            // Act
            _trafficManager._distances = new TwoDArray<float>(defaultDistances);
            _trafficManager.CountNonNeighborDistances();

            // Assert
            bool allDistancesSame = true;
            for (int i = 0; i < _trafficManager._distances.Height; i++)
            {
                for (int j = 0; j < _trafficManager._distances.Width; j++)
                {
                    if (i != j && _trafficManager._distances[i, j] != 1)
                    {
                        allDistancesSame = false;
                        break;
                    }
                }
                if (!allDistancesSame) break;
            }

            Assert.IsTrue(allDistancesSame, "All distances should be 1 for directly connected zones.");
        }   

        [Test]
        public void NonNeighborDistances_RandomDistances()
        {
            /*
            *   Z0 == 5.5f == Z1 == 7f == Z2 == 1f == Z4
            *   | |                      //
            *    4   //======  2  ======//
            *   | | //
            *   Z3
                    */


            // Arrange
            float[,] defaultDistances = new float[,]
            {
                //                  Z0                    Z1                    Z2                    Z3                    Z4
                /*Z0*/ { 0                     , 1                     , float.PositiveInfinity, 1                      , float.PositiveInfinity},
                /*Z1*/ { 5.5f                  , 0                     , 1                     , float.PositiveInfinity , float.PositiveInfinity},
                /*Z2*/ { float.PositiveInfinity, 7                     , 0                     , 1                      , 1},
                /*Z3*/ { 4                     , float.PositiveInfinity, 2                     , 0                      , float.PositiveInfinity},
                /*Z4*/ { float.PositiveInfinity, float.PositiveInfinity, 1                     , float.PositiveInfinity , 0},
            };

            float[,] expectedDistances = new float[,]
            {
              //                 Z0                    Z1                    Z2                    Z3                    Z4
              /*Z0*/ { 0                     , 1                     , 2                     , 1                      , 3},
              /*Z1*/ { 5.5f                  , 0                     , 1                     , 2                      , 2},
              /*Z2*/ { 6                     , 7                     , 0                     , 1                      , 1},
              /*Z3*/ { 4                     , 9                     , 2                     , 0                      , 2},
              /*Z4*/ { 7                     , 8                     , 1                     , 3                      , 0}                  
            };

            // Act
            _trafficManager._distances = new TwoDArray<float>(defaultDistances);
            _trafficManager.CountNonNeighborDistances();

            // Assert
            bool allDistancesCorrect = true;
            for (int i = 0; i < _trafficManager._distances.Height; i++)
            {
                for(int j = 0; j < _trafficManager._distances.Width; j++)
                {
                    if (_trafficManager._distances[i, j] != expectedDistances[i, j])
                    {
                        allDistancesCorrect = false;
                        break;
                    }
                }
            }
            Assert.IsTrue(allDistancesCorrect, "Distances aren't calculated correctly.");      
        } 
    
        [Test]
        public void CompareNeighDistance_CorrectResults()
        {
            // Arrange
            float[,] distances = new float[,]
            {
              //                 Z0                    Z1                    Z2                    Z3                    Z4
              /*Z0*/ { 0                     , 1                     , 2                     , 1                      , 3},
              /*Z1*/ { 5.5f                  , 0                     , 1                     , 2                      , 2},
              /*Z2*/ { 6                     , 7                     , 0                     , 1                      , 1},
              /*Z3*/ { 4                     , 9                     , 2                     , 0                      , 2},
              /*Z4*/ { 7                     , 8                     , 1                     , 3                      , 0}                  
            };


            var trafficZoneA = new GameObject("ZoneA").AddComponent<TrafficZone>();
            var trafficZoneB = new GameObject("ZoneB").AddComponent<TrafficZone>();

            var trafficZoneC = new GameObject("ZoneC").AddComponent<TrafficZone>();
            var trafficZoneD = new GameObject("ZoneD").AddComponent<TrafficZone>();
            
            var trafficZoneE = new GameObject("ZoneE").AddComponent<TrafficZone>();
            var trafficZoneF = new GameObject("ZoneF").AddComponent<TrafficZone>();


            var trafficZoneTo = new GameObject("ZoneTo").AddComponent<TrafficZone>();
            var trafficZoneTo2 = new GameObject("ZoneTo2").AddComponent<TrafficZone>();
            var trafficZoneTo3 = new GameObject("ZoneTo3").AddComponent<TrafficZone>();

            trafficZoneA.ID = 0; // Neighbor distance to Z4 = 3
            trafficZoneB.ID = 1; // Neighbor distance to Z4 = 2
            trafficZoneTo.ID = 4;

            trafficZoneC.ID = 3; // Neighbor distance to Z4 = 1
            trafficZoneD.ID = 2; // Neighbor distance to Z4 = 2
            trafficZoneTo2.ID = 0;

            trafficZoneE.ID = 0;
            trafficZoneF.ID = 2;
            trafficZoneTo3.ID = 3;

            _trafficManager._distances = new TwoDArray<float>(distances);

            // Act
            int comparisonResult1 = _trafficManager.CompareNeighDistance(trafficZoneA, trafficZoneB, trafficZoneTo);
            int comparisonResult2 = _trafficManager.CompareNeighDistance(trafficZoneC, trafficZoneD, trafficZoneTo2);
            int comparisonResult3 = _trafficManager.CompareNeighDistance(trafficZoneE, trafficZoneF, trafficZoneTo3);

            // Assert
            Assert.AreEqual(comparisonResult1, 1, "ZoneA should be considered farther from ZoneTo than ZoneB based on neighbor distance.");
            Assert.AreEqual(comparisonResult2, -1, "ZoneC should be considered farther from ZoneTo2 than ZoneD based on neighbor distance.");
            Assert.AreEqual(comparisonResult3, 0, "ZoneE and ZoneF should be considered equally far from ZoneTo3 based on neighbor distance.");
        }

        [Test]
        public void CompareNeighDistance_Commutative()
        {
            // Arrange
            float[,] distances = new float[,]
            {
              //                 Z0                    Z1                    Z2                    Z3                    Z4
              /*Z0*/ { 0                     , 1                     , 2                     , 1                      , 3},
              /*Z1*/ { 5.5f                  , 0                     , 1                     , 2                      , 2},
              /*Z2*/ { 6                     , 7                     , 0                     , 1                      , 1},
              /*Z3*/ { 4                     , 9                     , 2                     , 0                      , 2},
              /*Z4*/ { 7                     , 8                     , 1                     , 3                      , 0}                  
            };

            var trafficZoneA = new GameObject("ZoneA").AddComponent<TrafficZone>();
            var trafficZoneB = new GameObject("ZoneB").AddComponent<TrafficZone>();
            var trafficZoneTo = new GameObject("ZoneTo").AddComponent<TrafficZone>();

            trafficZoneA.ID = 0; // Neighbor distance to Z4 = 3
            trafficZoneB.ID = 1; // Neighbor distance to Z4 = 2
            trafficZoneTo.ID = 4;

            _trafficManager._distances = new TwoDArray<float>(distances);

            // Act
            int comparisonResultAB = _trafficManager.CompareNeighDistance(trafficZoneA, trafficZoneB, trafficZoneTo);
            int comparisonResultBA = _trafficManager.CompareNeighDistance(trafficZoneB, trafficZoneA, trafficZoneTo);

            // Assert
            Assert.AreEqual(comparisonResultAB, -comparisonResultBA, "Comparison should be commutative: if ZoneA is farther than ZoneB, then ZoneB should be closer than ZoneA.");
        }

        [Test]
        public void CompareNeighDistance_Reflexive()
        {
            // Arrange
            float[,] distances = new float[,]
            {
              //                 Z0                    Z1                    Z2                    Z3                    Z4
              /*Z0*/ { 0                     , 1                     , 2                     , 1                      , 3},
              /*Z1*/ { 5.5f                  , 0                     , 1                     , 2                      , 2},
              /*Z2*/ { 6                     , 7                     , 0                     , 1                      , 1},
              /*Z3*/ { 4                     , 9                     , 2                     , 0                      , 2},
              /*Z4*/ { 7                     , 8                     , 1                     , 3                      , 0}                  
            };

            var trafficZoneA = new GameObject("ZoneA").AddComponent<TrafficZone>();
            var trafficZoneTo = new GameObject("ZoneTo").AddComponent<TrafficZone>();

            trafficZoneA.ID = 0; // Neighbor distance to Z4 = 3
            trafficZoneTo.ID = 4;

            _trafficManager._distances = new TwoDArray<float>(distances);

            // Act
            int comparisonResultAA = _trafficManager.CompareNeighDistance(trafficZoneA, trafficZoneA, trafficZoneTo);

            // Assert
            Assert.AreEqual(comparisonResultAA, 0, "Comparison should be reflexive: a zone should be considered equally far from the target as itself.");   
        }
    
        [Test]
        public void SortNeighbors_CorrectOrder()
        {
            // Arrange
            float[,] distances = new float[,]
            {
              //                 Z0                    Z1                    Z2                    Z3                    Z4
              /*Z0*/ { 0                     , 1                     , 2                     , 1                      , 3},
              /*Z1*/ { 5.5f                  , 0                     , 1                     , 2                      , 2},
              /*Z2*/ { 6                     , 7                     , 0                     , 1                      , 1},
              /*Z3*/ { 4                     , 9                     , 2                     , 0                      , 2},
              /*Z4*/ { 7                     , 8                     , 1                     , 3                      , 0}                  
            };

            var trafficZoneFrom = new GameObject("ZoneFrom").AddComponent<TrafficZone>();
            var trafficZoneTo = new GameObject("ZoneTo").AddComponent<TrafficZone>();

            trafficZoneFrom.ID = 0; // Neighbor distances to Z4: Z1=1, Z3=1, Z2=2
            trafficZoneTo.ID = 4;

            var neighbor1 = new GameObject("Neighbor1").AddComponent<TrafficZone>(); // ID = 1, distance to target = 2
            var neighbor2 = new GameObject("Neighbor2").AddComponent<TrafficZone>(); // ID = 2, distance to target = 1
            var neighbor3 = new GameObject("Neighbor3").AddComponent<TrafficZone>(); // ID = 3, distance to target = 2

            neighbor1.ID = 1;
            neighbor2.ID = 2;
            neighbor3.ID = 3;


            neighbor1._enemies = new (){null, null}; // 2 enemies
            neighbor2._enemies = new (){null}; // 1 enemy
            neighbor3._enemies = new (){null}; // 1 enemy

            List<TrafficZone> neighbors = new List<TrafficZone>() { neighbor1, neighbor2, neighbor3 };

            // Neighbor2 and 3 have the least enemies. But neighbor2 has a shorter distance to the target, so it should be first. Neighbor1 and 3 have the same distance to the target, but neighbor3 has fewer enemies, so it should be second.
            List<TrafficZone> expectedOrder = new List<TrafficZone>() { neighbor2, neighbor3, neighbor1 }; 

            _trafficManager._distances = new TwoDArray<float>(distances);

            // Act
            var sortedNeighbors = _trafficManager.SortNeighbors(neighbors, trafficZoneFrom, trafficZoneTo, 0, neighbors.Count,SortingRule.EN_NDIST_EUCL);

            Debug.Log("Sorted neighbors order: " + string.Join(", ", sortedNeighbors.Select(n => n.name)));

            // Assert
            bool isCorrectOrder = sortedNeighbors.SequenceEqual(expectedOrder);
            

            Assert.IsTrue(isCorrectOrder, "Neighbors are not sorted in the correct order based on distance to target and number of enemies.");
        }


    }
}
