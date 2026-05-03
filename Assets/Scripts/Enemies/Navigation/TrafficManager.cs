using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using MonkeyBusiness.Misc;
using UnityEditor;
using System.IO;
using Sirenix.Utilities;

[assembly: InternalsVisibleTo("MonkeyBusiness.Tests")]
namespace MonkeyBusiness.Enemies.Navigation
{
    public enum SortingRule
    {
        /// <summary>
        /// First sorts by neighbor distance to target zone, then by number of enemies, finally by Euclidean distance to source zone.
        /// </summary>
        [Tooltip("First sorts by neighbor distance to target zone, then by number of enemies, finally by Euclidean distance to source zone.")]
        NDIST_EN_EUCL,
        [Tooltip("First sorts by number of enemies in the zone, then by neighbor distance to target zone, finally by Euclidean distance to source zone.")]
        EN_NDIST_EUCL,
        [Tooltip("First sorts by number of enemies in the zone and neighbor distance to target zone, then by Euclidean distance to source zone.")]
        ENANDNDIST_EUCL
    }

    [ExecuteAlways]
    public class TrafficManager : MonoBehaviour
    {
        public static TrafficManager Instance { get; private set; }

        [EnumButtons]
        [SerializeField]
        [Tooltip("Sorting rule to use when choosing the best neighbor")]
        internal SortingRule _sortingRule = SortingRule.NDIST_EN_EUCL;


        [EnumButtons]
        [SerializeField]
        [Tooltip("Fallback sorting rule if the road couldn't be found using the previous one")]
        internal SortingRule _fallbackRule = SortingRule.NDIST_EN_EUCL;

        [SerializeField]
        bool _hardResetOnSoft = false;

        [ReadOnly]
        [SerializeField]
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
        //[TableMatrix(HorizontalTitle = "Neighbor distance", VerticalTitle = "Euclidean distance", Transpose = true)]
        [SerializeField]
        internal TwoDArray<float> _distances;

        /// <summary>
        /// Event invoked when the player moves to a non-neighboring zone, forcing all enemies to recalculate their path.
        /// </summary>
        public UnityEvent OnHardRepath = new UnityEvent();

        public UnityEvent OnSoftRepath = new UnityEvent();

        [ShowInInspector]
        TrafficZone _playerZone = null;

        [field:SerializeField]
        public GameObject Player {get; private set;}

        public TrafficZone PlayerZone
        {
            get => _playerZone;
            set
            {
                var oldZone = _playerZone;
                _playerZone = value;
                if(oldZone == value)
                {
                    Debug.LogWarning("Player entered the same zone twice, might not be desired unless player left all zones");
                }
                if(oldZone != null && oldZone.Neighbors.Contains(value))
                {
                    if(_hardResetOnSoft)
                    {
                        OnHardRepath.Invoke();
                    }
                    else OnSoftRepath.Invoke();
                }
                else
                {
                    OnHardRepath.Invoke();
                }
            }
        }

        [Button("Connect", DisplayParameters = true)]
        internal void Connect(TrafficZone a, TrafficZone b)
        {
            a.AddNeighbor(b);
            b.AddNeighbor(a);
        }

        /// <summary>
        /// Tries altering the path without removing most of it, returns the result success.  
        /// </summary>
        public bool TryAlterPath(EnemyFollowController enemy, TrafficZone goal)
        {
            bool preserved = false;
            var path = enemy.Path;

            if(path.Count > 1 &&goal == path[path.Count - 2]) // If the end zone is the same as the previous end zone, we can preserve the path (except for the last zone, which is now different)
            {
                preserved = true;
                var lastZone = path[path.Count - 1];
                path.RemoveAt(path.Count - 1);
            }
            else if(path.Count > 0 && path[path.Count - 1].Neighbors.Contains(goal))
            {
                preserved = true;
                path.Add(goal);
            }
            return preserved;
        }   

        internal void ClearPath(EnemyFollowController enemy)
        {
            var path = enemy.Path;
            for(int i = path.Count - 1; i >= 0; i--)
            {
                path[i].RemoveEnemy(enemy);
                path.RemoveAt(i);
            }
        }

        /// <summary>
        /// Finds a path from <paramref name="start"/> to <paramref name="goal"/> and saves it into the <paramref name="path"/> list.
        /// </summary>
        public void GetPath(EnemyFollowController enemy, TrafficZone goal)
        {
            var path = enemy.Path;
            var position = enemy.transform.position;
            var start = enemy.CurrentZone;

            if(path.Count > 0)
                ClearPath(enemy);
            
            if(!FindPath(enemy, start, goal, position, path, _sortingRule))
            {
                if(!FindPath(enemy, start, goal, position, path, _fallbackRule))
                {
                    Debug.LogError("Couldn't find a path from " + start.ID + " to " + goal.ID + " for enemy " + enemy.name);
                }
            }
            
        }



        internal bool FindPath(EnemyFollowController enemy, TrafficZone start, TrafficZone goal, Vector3 customPos, List<TrafficZone> path, SortingRule rule)
        {
            var nextZone = GetBestNeighbor(start, goal, customPos, path, rule);
            if(nextZone == null)
            {
                ClearPath(enemy);
                return false;
            }

            path.Add(nextZone);
            nextZone.AddEnemy(enemy);
    
            while(nextZone != goal)
            {
                nextZone = GetBestNeighbor(nextZone, goal, path, rule);
                if(nextZone == null)
                {
                    ClearPath(enemy);
                    return false;
                }
                nextZone.AddEnemy(enemy);
                path.Add(nextZone);
            }
            return true;
        }

        internal int CompareNeighbors(TrafficZone a, TrafficZone b, TrafficZone from, TrafficZone to, SortingRule rule)
        {
            if(a == b) return 0; // If A and B are the same, they are equal
            if(a == to) return -1; // If A is the target, it should be first
            if(b == to) return 1; // If B is the target, it should be first

            var neighDistComparison = CompareNeighDistance(a, b, to);
            var enemyComparison = CompareNumEnemies(a, b);
            var euclComparison = CompareEuclideanDistance(a, b, from);

            switch(rule)
            {
                case SortingRule.NDIST_EN_EUCL:
                    if(neighDistComparison != 0) return neighDistComparison;
                    if(enemyComparison != 0) return enemyComparison;
                    return euclComparison;
                case SortingRule.EN_NDIST_EUCL:
                    if(enemyComparison != 0) return enemyComparison;
                    if(neighDistComparison != 0) return neighDistComparison;
                    return euclComparison;
                case SortingRule.ENANDNDIST_EUCL:
                    var combinedComparison = CompareEnemiesAndNeighDistance(a, b, to);
                    if(combinedComparison != 0) return combinedComparison;
                    return euclComparison;
                default:
                    throw new InvalidOperationException("Invalid sorting rule: " + _sortingRule);
            }
        }
        internal int CompareNeighbors(TrafficZone a, TrafficZone b, TrafficZone to, Vector3 customPos, SortingRule rule)
        {
            if(a == b) return 0; // If A and B are the same, they are equal
            if(a == to) return -1; // If A is the target, it should be first
            if(b == to) return 1; // If B is the target, it should be first

            var neighDistComparison = CompareNeighDistance(a, b, to);
            var enemyComparison = CompareNumEnemies(a, b);
            var euclComparison = CompareEuclideanDistance(a, b, customPos);

            switch(rule)
            {
                case SortingRule.NDIST_EN_EUCL:
                    if(neighDistComparison != 0) return neighDistComparison;
                    if(enemyComparison != 0) return enemyComparison;
                    return euclComparison;
                case SortingRule.EN_NDIST_EUCL:
                    if(enemyComparison != 0) return enemyComparison;
                    if(neighDistComparison != 0) return neighDistComparison;
                    return euclComparison;
                case SortingRule.ENANDNDIST_EUCL:
                    var combinedComparison = CompareEnemiesAndNeighDistance(a, b, to);
                    if(combinedComparison != 0) return combinedComparison;
                    return euclComparison;
                default:
                    throw new InvalidOperationException("Invalid sorting rule: " + rule);
            }
        }
        internal int CompareNumEnemies(TrafficZone a, TrafficZone b)
        {
            Debug.Log("Comparing num enemies: A= " + a.NumEnemies + ", B= " + b.NumEnemies);

            if(a.NumEnemies < b.NumEnemies)
            {
                Debug.Log("Num enemies - A has less");
                return -1;
            }
            else if(a.NumEnemies > b.NumEnemies)
            {
                Debug.Log("Num enemies - B has less");
                return 1;
            }
            else
            {
                Debug.Log("Num enemies - A and B are equal");
                return 0;
            } 
                
        } 

        /// <summary>
        /// Compares A and B by their neighbor distance to the target zone.
        /// </summary>
        internal int CompareNeighDistance(TrafficZone a, TrafficZone b, TrafficZone to)
        {
            var neighA = GetNeighborDistance(a.ID, to.ID);
            var neighB = GetNeighborDistance(b.ID, to.ID);

            Debug.Log("Neigh distance: A= " + neighA + ", B= " + neighB);

            if(neighA < neighB)
            {
                Debug.Log("Neigh distance - A is less");
                return -1;
            }
            else if(neighA > neighB)
            {
                Debug.Log("Neigh distance - B is less");
                return 1;
            }
            else
            {
                Debug.Log("Neigh distance - A and B are equal");
                return 0;
            } 
        }

        internal int CompareEnemiesAndNeighDistance(TrafficZone a, TrafficZone b, TrafficZone to)
        {
            var neighA = GetNeighborDistance(a.ID, to.ID);
            var neighB = GetNeighborDistance(b.ID, to.ID);

            var enemiesA = a.NumEnemies;
            var enemiesB = b.NumEnemies;

            if(enemiesA + neighA < enemiesB + neighB)
            {
                return -1;
            }
            else if(enemiesA + neighA > enemiesB + neighB)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Compares A and B by their Euclidean distance to the prevoius traffic zone.
        /// </summary>
        internal int CompareEuclideanDistance(TrafficZone a, TrafficZone b, TrafficZone from)
        {
            var distA = GetEuclideanDistance(a.ID, from.ID);
            var distB = GetEuclideanDistance(b.ID, from.ID);
            if(distA < distB)
            {
                return -1;
            }
            else if(distA > distB)
            {
                return 1;
            }
            else return 0;
        }

        /// <summary>
        /// Compares A and B by their Euclidean distance to a custom position.
        /// </summary>
        internal int CompareEuclideanDistance(TrafficZone a, TrafficZone b, Vector3 from)
        {
            var distA = Vector3.Distance(a.Keypoint.position, from);
            var distB = Vector3.Distance(b.Keypoint.position, from);
            if(distA < distB)
            {
                return -1;
            }
            else if(distA > distB)
            {
                return 1;
            }
            else return 0;
        }

        TrafficZone GetBestNeighbor(TrafficZone from, TrafficZone to, Vector3 customPos, List<TrafficZone> path, SortingRule rule)
        {
            if(from.Neighbors.Contains(to)) return to;
            List<TrafficZone> possibleNeighbors = new List<TrafficZone>(from.Neighbors);
            var sortedNeighbors = SortNeighbors(possibleNeighbors, to, customPos, 0, possibleNeighbors.Count, rule);

            for(int i = 0; i < sortedNeighbors.Count; i++)
            {
                if(!path.Contains(sortedNeighbors[i]))
                {
                    return sortedNeighbors[i];
                }
            }
            return null; // No valid path
        }

        TrafficZone GetBestNeighbor(TrafficZone from, TrafficZone to, List<TrafficZone> path, SortingRule rule)
        {
            if(from.Neighbors.Contains(to)) return to;  
            List<TrafficZone> possibleNeighbors = new List<TrafficZone>(from.Neighbors);
            var sortedNeighbors = SortNeighbors(possibleNeighbors, from, to, 0, possibleNeighbors.Count, rule);

            for(int i = 0; i < sortedNeighbors.Count; i++)
            {
                if(!path.Contains(sortedNeighbors[i]))
                {
                    return sortedNeighbors[i];
                }
            }
            return null; // No valid path     
        }

        /// <summary>
        /// Recalculates the distance matrix.
        /// </summary>
        /// <remarks><b>Shouldn't be called during runtime!</b></remarks>
        internal void RecalculateDistance()
        {
            _distances = new TwoDArray<float>(_zones.Length, _zones.Length);
            CountNeighborDistances();

            CountNonNeighborDistances();
            #if UNITY_EDITOR
                EditorUtility.SetDirty(this);
            #endif
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
            for(int k = 0; k < _distances.Height - 2; k++) // N-2 iterations, for max distance is N-1 and we already setup the base distance
            {
                for(int i = 0; i < _distances.Height; i++) // For each row
                {
                    for(int j = i+1; j < _distances.Width; j++) // For each column (totally for each cell)
                    {
                        // The minimal distance = min(current distance, distance to neighbor + neighbor's distance to target)
                        for(int m = 0; m < _distances.Width; m++)
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

            if(Application.isPlaying) Debug.LogError("CALLING EDITOR-ONLY FUNCTION DURING RUNTIME");
            ResetZones();
            FindAndConnectZones();
            RecalculateDistance();        
            #if UNITY_EDITOR
                EditorUtility.SetDirty(this);
            #endif
        }

        [Button("Reset zones")]
        public void ResetZones()
        {
            if(Application.isPlaying) Debug.LogError("CALLING EDITOR-ONLY FUNCTION DURING RUNTIME");
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

        public TrafficZone GetZone(Vector3 position)
        {
            foreach(var zone in _zones)
            {
                if(zone.ZoneBounds.Contains(position))
                {
                    return zone;
                }
            }
            return null;
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

                    #if UNITY_EDITOR
                        EditorUtility.SetDirty(zone_j);
                    #endif
                }

                #if UNITY_EDITOR
                    EditorUtility.SetDirty(zone_i);
                #endif
            }
        }

        public void ForceRepath()
        {
            foreach(var zone in _zones)
            {
                zone.ClearEnemies();
            }

            OnHardRepath.Invoke();
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

            var gm = GetComponent<ITargetable>();
            if(gm == null) Debug.LogError("Didn't find GameManager on this GameObject - you should put TrafficManager and GameManager on the same object.");
            else Player = gm.Target;

            Instance = this;
        }

        internal List<TrafficZone> SortNeighbors(List<TrafficZone> neighbors, TrafficZone from, TrafficZone to, int startInclusive, int endExclusive, SortingRule rule)
        {
            // Merge sort
            if(endExclusive - startInclusive <= 1) return new List<TrafficZone> { neighbors[startInclusive] };

            int mid = (startInclusive + endExclusive) / 2;

            var left = SortNeighbors(neighbors, from, to, startInclusive, mid, rule);
            var right = SortNeighbors(neighbors, from, to, mid, endExclusive, rule);

            int i = 0;
            int j = 0;

            List<TrafficZone> sorted = new List<TrafficZone>();

            while(i < left.Count || j < right.Count)
            {

                int comparisonResult = i == left.Count ?
                    1 : 
                    j == right.Count ?
                        -1 : 
                        CompareNeighbors(left[i], right[j], from, to, rule);

                switch(comparisonResult)
                {
                    case -1: // Left is less, already in correct order
                        sorted.Add(left[i]);
                        i++;
                        break;
                    case 1: // Right is less, needs to be moved before left
                        sorted.Add(right[j]);
                        j++;
                        break;
                    case 0: // Equal, can choose either one to go first, let's say left
                        sorted.Add(left[i]);
                        i++;
                        break;
                }
            }

            return sorted;
        }

        internal List<TrafficZone> SortNeighbors(List<TrafficZone> neighbors, TrafficZone to, Vector3 customPos, int startInclusive, int endExclusive, SortingRule rule)
        {
            // Merge sort
            if(endExclusive - startInclusive <= 1) return new List<TrafficZone> { neighbors[startInclusive] };

            int mid = (startInclusive + endExclusive) / 2;

            var left = SortNeighbors(neighbors, to, customPos, startInclusive, mid, rule);
            var right = SortNeighbors(neighbors, to, customPos, mid, endExclusive, rule);

            int i = 0;
            int j = 0;

            List<TrafficZone> sorted = new List<TrafficZone>();

            while(i < left.Count || j < right.Count)
            {

                int comparisonResult = i == left.Count ?
                    1 : 
                    j == right.Count ?
                        -1 : 
                        CompareNeighbors(left[i], right[j], to, customPos, rule);

                switch(comparisonResult)
                {
                    case -1: // Left is less, already in correct order
                        sorted.Add(left[i]);
                        i++;
                        break;
                    case 1: // Right is less, needs to be moved before left
                        sorted.Add(right[j]);
                        j++;
                        break;
                    case 0: // Equal, can choose either one to go first, let's say left
                        sorted.Add(left[i]);
                        i++;
                        break;
                }
            }

            return sorted;
        }
    }
}
