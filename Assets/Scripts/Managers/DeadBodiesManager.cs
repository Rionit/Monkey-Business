using MonkeyBusiness.Combat.Health;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace MonkeyBusiness.Managers
{
    public class DeadBodiesManager : MonoBehaviour
    {
        public static int MaxDeadBodies = 200;

        public static DeadBodiesManager Instance { get; private set; }

        List<EnemyDeathController> _deadBodies = new List<EnemyDeathController>();

        int _currentBodyIndex = 0;

        public void Start()
        {
            Instance = this;

            if(GameManager.Instance != null)
            {
                GameManager.Instance.OnPausedOrUnpaused.AddListener(UpdateDeadBodies);
            }
            _deadBodies = new List<EnemyDeathController>(MaxDeadBodies);

            for(int i = 0; i < MaxDeadBodies; i++)
            {
                _deadBodies.Add(null);
            }

            Debug.Log("Setting up dead bodies to count " + MaxDeadBodies + " ... actual count: " + _deadBodies.Count);
        }

        void OnDestroy()
        {
            if(GameManager.Instance != null)
            {
                GameManager.Instance.OnPausedOrUnpaused.RemoveListener(UpdateDeadBodies);
            }
        }

        public void ChangeMaxDeadBodies(float newVal)
        {
            MaxDeadBodies = Mathf.RoundToInt(newVal);
        }

        public void UpdateDeadBodies(bool paused)
        {
            if(!paused)
            {
                if(_deadBodies.Count != MaxDeadBodies) Debug.Log("Changing max dead bodies to " + MaxDeadBodies + " ... actual count: " + _deadBodies.Count);
                if(_deadBodies.Count > MaxDeadBodies)
                {
                    for(int i = _deadBodies.Count -1; i >= 0 && _deadBodies.Count > MaxDeadBodies; i--)
                    {
                        Debug.Log("Destroying body");
                        if(_deadBodies[i] != null) _deadBodies[i].DestroyBody();
                        _deadBodies.RemoveAt(i);
                    }

                    _currentBodyIndex = Math.Min(_currentBodyIndex, _deadBodies.Count - 1);
                    Debug.Log("Reducing dead bodies to max limit: " + MaxDeadBodies + " ... new count: " + _deadBodies.Count);
                }
                else if(_deadBodies.Count < MaxDeadBodies)
                {
                    for(int i = _deadBodies.Count; i < MaxDeadBodies; i++)
                    {
                        Debug.Log("Adding null body to list");
                        _deadBodies.Add(null);
                    }

                    Debug.Log("Increasing dead bodies to max limit: " + MaxDeadBodies + " ... new count: " + _deadBodies.Count);
                }
            }
        }

        public void AddDeadBody(EnemyDeathController body)
        {
            if(MaxDeadBodies > 0)
            {
                _currentBodyIndex = (_currentBodyIndex + 1) % MaxDeadBodies;
                Debug.Log("Adding body at index " + _currentBodyIndex);

                if(_deadBodies[_currentBodyIndex] != null)
                {
                    _deadBodies[_currentBodyIndex].DestroyBody();
                }
                
                _deadBodies[_currentBodyIndex] = body;
            }
            else if(MaxDeadBodies == 0)
            {
                Debug.Log("Destroying body");
                body.DestroyBody();
            }
        }
    }
}
