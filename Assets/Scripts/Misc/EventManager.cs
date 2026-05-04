using UnityEngine;
using UnityEngine.Events;

namespace MonkeyBusiness.Misc
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        public UnityEvent<float> OnEnemyHit;
    
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of StatsManager detected! Replacing the old one.");
            }
            Instance = this;
        }
    }
}
