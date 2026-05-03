using System.Collections.Generic;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace MonkeyBusiness.Enemies.Navigation
{
    /// <summary>
    /// Defines a traffic zone for enemies to navigate through.
    /// </summary>
    [ExecuteInEditMode]
    public class TrafficZone : MonoBehaviour
    {

        [ShowInInspector]
        [FoldoutGroup("Zone Bounds")]
        internal Vector3 _Center
        {
            get
            {
                if(_zoneCollider == null)
                {
                    return Vector3.positiveInfinity;
                }
                return _zoneCollider.center;
            }

            set
            {
                if(_zoneCollider == null)
                {
                    Debug.LogWarning("Zone collider is not assigned.");
                    return;
                }
                _zoneCollider.center = value;
            }
        }

        [ShowInInspector]
        [FoldoutGroup("Zone Bounds")]
        internal Vector3 _Size
        {
            get
            {
                if(_zoneCollider == null)
                {
                    return Vector3.positiveInfinity;
                }
                return _zoneCollider.size;
            }

            set
            {
                if(_zoneCollider == null)
                {
                    Debug.LogWarning("Zone collider is not assigned.");
                    return;
                }
                _zoneCollider.size = value;
            }
        }

        
        /// <summary>
        /// Bounds of the zone.
        /// </summary>
        /// <remarks> <i> For automatic zone connection, colliders of neighboring zones should intersect. </i> </remarks>
        [ShowInInspector]
        [Tooltip("Bounds of the zone. \n\n <i> For automatic zone connection, colliders of neighboring zones should intersect. </i>")]
        public Bounds ZoneBounds
        {
            get
            {
                if (_zoneCollider == null)
                {
                    Debug.LogWarning("Zone collider is not assigned.");
                    return new Bounds();
                }
                return _zoneCollider.bounds;
            }
        }



        /// <summary>
        /// ID of the zone. Used for debugging and visualization purposes.
        /// </summary>
        [field: ReadOnly]
        [field: SerializeField]
        public int ID { get; set; } = -1;

        /// <summary>
        /// Keypoint for enemies to path towards when navigating through the zone.
        /// </summary>
        [field:SerializeField]
        public Transform Keypoint {get; internal set;}

        [field: ReadOnly]
        [field: Tooltip("Zones neighboring current zone.")]
        [field: SerializeField]
        public List<TrafficZone> Neighbors {get; private set;} = new List<TrafficZone>();

        [ShowInInspector]
        [ReadOnly]
        [Tooltip("Enemies pathing through the zone.")]
        internal List<EnemyFollowController> _enemies = new List<EnemyFollowController>();

        [Required]
        [SerializeField]
        internal BoxCollider _zoneCollider;

        #if UNITY_EDITOR
        [BoxGroup("Debug")]
        [ShowInInspector]
        bool _showZoneBounds = false;
        #endif

        public int NumEnemies => _enemies.Count;

        /// <summary>
        /// Adds the <paramref name="enemy"/> to the zone.
        /// </summary>
        public void AddEnemy(EnemyFollowController enemy)
        {
            if(!_enemies.Contains(enemy))
                _enemies.Add(enemy);
            else Debug.LogWarning("Trying to add an already present enemy to the zone " + ID);
        }

        /// <summary>
        /// Clears the enemy list for the zone.
        /// </summary>
        public void ClearEnemies() => _enemies.Clear();

        /// <summary>
        /// Removes the <paramref name="enemy"/> from the zone.
        /// </summary>
        /// <param name="enemy"></param>
        public void RemoveEnemy(EnemyFollowController enemy)
        {
            if(_enemies.Contains(enemy))
                _enemies.Remove(enemy);
            else Debug.LogWarning("Trying to remove an enemy that is not present in the zone " + ID);
        }

        public void AddNeighbor(TrafficZone neighbor)
        {
            if (!Neighbors.Contains(neighbor))
                Neighbors.Add(neighbor);
            else Debug.LogWarning("Trying to add an already present neighbor to the zone " + ID);
        }

        public void RemoveNeighbor(TrafficZone neighbor)
        {
            if (Neighbors.Contains(neighbor))
                Neighbors.Remove(neighbor);
            else Debug.LogWarning("Trying to remove a neighbor that is not present in the zone " + ID);
        }

        void OnTriggerEnter(Collider other)
        {
            var tag = other.tag;
            if(tag == "Enemy")
            {
                var followController = other.GetComponentInParent<EnemyFollowController>();
                if(followController != null)
                {
                    followController.CurrentZone = this;
                    Debug.Log("Enemy entered zone " + ID);
                }
                else
                {
                    Debug.LogError("Enemy doesn't have EnemyFollowController");
                }
            }
            else if(tag == "Player")
            {
                TrafficManager.Instance.PlayerZone = this;
                Debug.Log("Player entered zone " + ID);
            }           
        }

        void Start()
        {
            if(Application.isPlaying && ID == -1)
            {
                Debug.LogError("Zone " + name + " doesn't have an ID assigned. In GameManager, click on 'Setup Zone' before entering play mode!");
            }
        }

        void OnGUI()
        {
            
            Handles.color = Color.red;
            foreach(var neighbor in Neighbors)
            {
                if(neighbor != null)
                    Handles.DrawLine(Keypoint.position, neighbor.Keypoint.position, 3f);
            }
        }

        void OnDrawGizmos()
        {            
            #if UNITY_EDITOR
            Gizmos.color = ID != -1 ? Color.gray : Color.red;
            Gizmos.DrawSphere(Keypoint.position, 0.5f);

            for (int i = 0; i < Neighbors.Count; i++)
            {
                bool nullNeighbor = Neighbors[i] == null;
                Handles.color = nullNeighbor ? Color.red : new Color(0f, 1f, 1f, 0.5f);
                var position = !nullNeighbor ? Neighbors[i].Keypoint.position : Vector3.up;

                Handles.DrawLine(Keypoint.position, position, 3f);
            }

            if(_showZoneBounds)
            {
                Gizmos.color = new Color(0.4f,0.4f, 1f, 0.5f);
                Gizmos.DrawCube(_Center + transform.position, _Size);
            }


            GUIStyle style = new GUIStyle();
            style.fontSize = 30;
            style.normal.textColor = Color.black;
            Handles.Label(Keypoint.position + Vector3.up, ID.ToString(), style);

            GUIStyle enemyCountStyle = new GUIStyle();
            enemyCountStyle.fontSize = 25;
            enemyCountStyle.normal.textColor = Color.darkRed;
            Handles.Label(Keypoint.position + Vector3.up + Vector3.back * 4f, NumEnemies.ToString(), enemyCountStyle);
            #endif
        }

        public override string ToString()
        {
            if(ID == -1)
            {
                return "Zone - ID NOT ASSIGNED";
            }
            else return "Zone " + ID;
        }
    }
}