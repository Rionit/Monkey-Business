using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UIElements;

namespace MonkeyBusiness.Enemies.Navigation
{


    public enum FlankingDirection
    {
        LEFT,
        RIGHT
    }


    public class FlankingController : MonoBehaviour
    {
        static readonly Vector3[] DIRECTION_VECTORS =
        {
            Vector3.left,
            Vector3.right
        };

        /// <remarks>
        /// Direction ordering: 0 = left, 1 = right, (2 = front, 3 = back ... if implemented). <br/>
        /// <i>Dictionary not used because of Unity serialization </i>
        /// </remarks>
        [SerializeField]
        [Tooltip("Direction ordering: 0 = left, 1 = right, (2 = front, 3 = back ... if implemented)")]
        Transform[] _flankingPoints;

        
        [SerializeField]
        [HideInInspector]
        float _flankingDistance = 5f;


        [SerializeField]
        [Tooltip("How much of the difference between the number of enemies flanking on each side is required for the controller to prefer the less flanked side")]
        int _flankingDisproportionThreshold = 3;

        [ShowInInspector]
        public float FlankingDistance
        {
            get => _flankingDistance;

            set
            { 
                _flankingDistance = value;
                if(_flankingPoints.Length < Enum.GetValues(typeof(FlankingDirection)).Length)
                {
                    Debug.LogError("Not enough flanking points assigned to the flanking controller");

                }
                // Sets the flanking points position as well
                for (int i = 0; i < Enum.GetValues(typeof(FlankingDirection)).Length; i++)
                {
                    _flankingPoints[i].localPosition = DIRECTION_VECTORS[i] * value;

                }
            }
        }

        Dictionary<FlankingDirection, HashSet<EnemyFollowController>> _flankingEnemies = new();
        
        void Awake()
        {
            for (int i = 0; i < Enum.GetValues(typeof(FlankingDirection)).Length; i++)
            {
                _flankingEnemies.Add((FlankingDirection)i, new HashSet<EnemyFollowController>());
            }
        }

        /// <summary>
        /// Returns the suitable flanking direction for the given follow controller. 
        /// </summary>
        /// <remarks><i>Returns either the direction with the fewest flanking enemies or the direction with the closest flanking enemy, depending on the disproportion threshold</i></remarks>
        FlankingDirection GetFlankingDirection(EnemyFollowController followController)
        {
            FlankingDirection min_count_direction = FlankingDirection.LEFT;
            FlankingDirection min_distance_direction = FlankingDirection.LEFT;
            int min_count = int.MaxValue;
            float min_distance = float.MaxValue;

            int max_count = int.MinValue;

            for(int i = 0; i < Enum.GetValues(typeof(FlankingDirection)).Length; i++)
            {
                var enemiesCount = _flankingEnemies[(FlankingDirection)i].Count;

                if(enemiesCount < min_count)
                {
                    min_count_direction = (FlankingDirection)i;
                    min_count = _flankingEnemies[min_count_direction].Count;
                }
                if(Vector3.Distance(followController.transform.position, _flankingPoints[i].position) < min_distance)
                {
                    min_distance_direction = (FlankingDirection)i;
                    min_distance = Vector3.Distance(followController.transform.position, _flankingPoints[i].position);
                }

                if(enemiesCount > max_count)
                {
                    max_count = enemiesCount;
                }
            }

            return (max_count - min_count >= _flankingDisproportionThreshold) ? min_count_direction : min_distance_direction;
        }

        public Transform GetMovementTarget(EnemyFollowController followController)
        {
            RemoveFromList(followController);
            if(Vector3.Distance(followController.transform.position, transform.position) < _flankingDistance)
            {
                return transform;
            }

            var direction = GetFlankingDirection(followController);
            _flankingEnemies[direction].Add(followController);
            return _flankingPoints[(int)direction];
        }

        public void RemoveFromList(EnemyFollowController followController)
        {
            // TODO: Optimize if needed 
            foreach(var enemyList in _flankingEnemies)
            {
                if(enemyList.Value.Contains(followController))
                {
                    enemyList.Value.Remove(followController);
                    return;
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 0f, 1f, 0.2f);
            Gizmos.DrawSphere(transform.position, _flankingDistance);

            if(_flankingPoints != null && _flankingPoints.Length >= Enum.GetValues(typeof(FlankingDirection)).Length)
            {
                for (int i = 0; i < Enum.GetValues(typeof(FlankingDirection)).Length; i++)
                {
                    Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
                    Gizmos.DrawLine(transform.position, transform.position + DIRECTION_VECTORS[i] * _flankingDistance);
                    Gizmos.DrawSphere(transform.position + DIRECTION_VECTORS[i] * _flankingDistance, 0.5f);
                }
            }
        }
    }
}
