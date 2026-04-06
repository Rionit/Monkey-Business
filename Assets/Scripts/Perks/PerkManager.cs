using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class PerkManager : MonoBehaviour
{
    [SerializeField] private GameObject perkPrefab;
    [SerializeField] private GameObject perkSelectionUI;
    [SerializeField] private List<PerkSO> perks;
    
    // Currently randomized perks
    private GameObject perk1, perk2, perk3;

    private void Start()
    {
        RandomizeNewPerks();
    }

    [Button]
    private void RandomizeNewPerks()
    {
        if(perk1 != null) Destroy(perk1);
        if(perk2 != null) Destroy(perk2);
        if(perk3 != null) Destroy(perk3);
        
        perk1 = InstantiatePerk(perks[Random.Range(0, perks.Count)]);
        perk2 = InstantiatePerk(perks[Random.Range(0, perks.Count)]);
        perk3 = InstantiatePerk(perks[Random.Range(0, perks.Count)]);
    }

    private GameObject InstantiatePerk(PerkSO perkSO)
    {
        GameObject instance = Instantiate(perkPrefab, Vector3.zero, Quaternion.identity, perkSelectionUI.transform);
        Perk perk = instance.GetComponent<Perk>();
        perk.Setup(perkSO);
        return instance;
    }
}
