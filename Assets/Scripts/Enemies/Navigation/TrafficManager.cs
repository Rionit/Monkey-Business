using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[assembly: InternalsVisibleTo("MonkeyBusiness.Tests")]
namespace MonkeyBusiness.Enemies.Navigation
{
    [ExecuteAlways]
    public class TrafficManager : MonoBehaviour
    {
        public static TrafficManager Instance { get; private set; }

        internal TrafficZone[] _zones;

        /// <summary>
        /// Neighboring (upper triangle) and Euclidean (lower triangle) distances between zones. Used for pathfinding. 
        /// </summary>
        /// <remarks>
        /// Example for 3 zones: <br/>
        /// <code lang="text">
        /// Zone 1 ←2.5→ Zone 2 ←2.1→ Zone 3
        /// | ====== | Zone 1 | Zone 2 | Zone 3 |
        /// | Zone 1 |   0    |   1    |   2    |
        /// | Zone 2 |  2.5   |   0    |   1    |
        /// | Zone 3 |  4.6   |   2.1  |   0    |
        /// </code> 
        /// </remarks>
        [ShowInInspector]
        [ReadOnly]
        [TableMatrix(HorizontalTitle = "Neighbor distance", VerticalTitle = "Euclidean distance", Transpose = true)]
        internal float[,] _distances;

        public UnityEvent OnForcedRepath = new UnityEvent();

        public TrafficZone PlayerZone { get; set; } = null;

        [Button("Connect", DisplayParameters = true)]
        internal void Connect(TrafficZone a, TrafficZone b)
        {
            a.AddNeighbor(b);
            b.AddNeighbor(a);
        }

        // TODO: Write unit tests.
        /// <summary>
        /// Recalculates the distance matrix.
        /// </summary>
        /// <remarks><b>Shouldn't be called during runtime!</b></remarks>
        internal void RecalculateDistance()
        {
            _distances = new float[_zones.Length, _zones.Length];
            CountNeighborDistances();

            CountNonNeighborDistances();
        }

        /// <remarks><b>Shouldn't be called during runtime!</b></remarks>
        internal void CountNeighborDistances()
        {
            for(int i = 0; i < _zones.Length; i++)
            {
                for(int j = i+1; j < _zones.Length; j++)
                {
                    var zone_i = _zones[i];
                    var zone_j = _zones[j];

                    bool neighbors = zone_i.Neighbors.Contains(zone_j);
                    float distance = neighbors ? Vector3.Distance(zone_i.Keypoint.position, zone_j.Keypoint.position) : float.PositiveInfinity;

                    _distances[i,j] = neighbors ? 1 : float.PositiveInfinity;
                    _distances[j,i] = distance;
                }
            }
        }

        /// <remarks><b>Shouldn't be called during runtime!</b></remarks>
        internal void CountNonNeighborDistances()
        {
            for(int k = 0; k < _distances.GetLength(0)- 2; k++) // N-2 iterations, for max distance is N-1 and we already setup the base distance
            {
                
                for(int i = 0; i < _distances.GetLength(0); i++) // For each row
                {
                    for(int j = i+1; j < _distances.GetLength(1); j++) // For each column (totally for each cell)
                    {
                        // The minimal distance = min(current distance, distance to neighbor + neighbor's distance to target)
                        for(int m = 0; m < _distances.GetLength(1); m++)
                        {
                            if(m == j) continue; // Skip self

                            var currentNDist = GetNeighborDistance(i, j);
                            var possibleNextNDist = GetNeighborDistance(i, m) + GetNeighborDistance(m, j);
                            if(currentNDist >= possibleNextNDist)
                            {
                                _distances[i,j] = possibleNextNDist; // Neighbor distance
                                _distances[j,i] = Mathf.Min(_distances[j,i], GetEuclideanDistance(i,m) + GetEuclideanDistance(m,j)); // Euclidean distance
                            }
                        }
                    }
                }
            }   // NOTE: Non-optimal time-wise, but this should be loaded in advance.
        }

        public float GetNeighborDistance(int from, int to)
        {
            if(from < to)
            {
                return _distances[from, to];
            }
            else
            {
                return _distances[to, from];
            }
        }

        public float GetEuclideanDistance(int from, int to)
        {
            if(from < to)
            {
                return _distances[to, from];
            }
            else
            {
                return _distances[from, to];
            }
        }

        [Button("Setup zones")]
        public void SetupZones()
        {
            ResetZones();
            FindAndConnectZones();
            RecalculateDistance();        
        }

        [Button("Reset zones")]
        public void ResetZones()
        {
            if(_zones != null)
            {
                foreach(var zone in _zones)
                {
                    zone.ID = -1;
                    zone.Neighbors.Clear();
                }
            }
            _zones = null;
            _distances = null;
        }

        /// <summary>
        /// Finds all zones in the scene, assigns them an ID and connects neighboring zones (zones with intersecting colliders). <br/>
        /// </summary>
        internal void FindAndConnectZones()
        {
            _zones = FindObjectsByType<TrafficZone>(FindObjectsSortMode.InstanceID);

            for(int i = 0; i < _zones.Length; i++)
            {
                var zone_i = _zones[i];
                zone_i.ID = i;

                for(int j = i+1; j < _zones.Length; j++)
                {
                    var zone_j = _zones[j];
                    if(zone_i.ZoneBounds.Intersects(zone_j.ZoneBounds))
                    {
                        Connect(zone_i, zone_j);
                    }
                }
            }
        }


        public void ForceRepath()
        {
            foreach(var zone in _zones)
            {
                zone.ClearEnemies();
            }

            OnForcedRepath.Invoke();
        }

        void Awake()
        {
            if(Instance != null)
            {
                if(Instance == this)
                {
                    Debug.Log("Instance of TrafficManager already exists, but is the same as us. Expected behavior");
                }
                else
                {
                    Debug.LogError("Other instance of TrafficManager already exists! Replacing it, but behavior is undefined");
                }
            }

            Instance = this;
        }
    }
}
