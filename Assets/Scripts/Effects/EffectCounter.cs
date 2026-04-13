using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MonkeyBusiness.Combat;
using System.Linq;

namespace MonkeyBusiness.Effects
{

    /// <summary>
    /// Scene-wide counter for active effects.
    /// </summary>
    public class EffectCounter : MonoBehaviour
    {
        public static EffectCounter Instance {get; private set; }
    
        void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of EffectCounter detected! Replacing the old one.");
            }
            Instance = this;
        }


        // TODO: Implement proper effect counting and management logic.
    }
}
