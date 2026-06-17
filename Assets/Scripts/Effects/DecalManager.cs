using UnityEngine;
using System.Collections.Generic;


namespace MonkeyBusiness.Effects
{
    public class DecalManager : MonoBehaviour
    {
        const int MAX_DECALS = 100;
      
        public static DecalManager Instance { get; private set; }

        List<DecalDecay> _activeDecals = new List<DecalDecay>(MAX_DECALS);

        int _currentDecalIndex = 0;

        void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of DecalManager detected! Replacing the old one.");
            }
            Instance = this;
        }

        public int RegisterNewDecal(DecalDecay decal)
        {
            if(_currentDecalIndex >= _activeDecals.Count)
            {
                _activeDecals.Add(decal);
            }
            else
            {
                Debug.Log("Removing decal: " + _currentDecalIndex);
                if(_activeDecals[_currentDecalIndex] != null)
                {
                    _activeDecals[_currentDecalIndex].OnDecayed.RemoveListener(UnregisterDecal);
                    _activeDecals[_currentDecalIndex].RemoveImmediately();
                }
                
                _activeDecals[_currentDecalIndex] = decal;
            }
            int registeredIndex = _currentDecalIndex;
            _currentDecalIndex = (_currentDecalIndex + 1) % MAX_DECALS;

            decal.OnDecayed.AddListener(UnregisterDecal);

            return registeredIndex;
        }

        void UnregisterDecal(int index)
        {
            _activeDecals[index] = null;
        }
    }
}
