using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class PerkManager : MonoBehaviour
{
    public UnityEvent OnPerkSelected = new();
    
    [BoxGroup("Perk Manager Setup"), Required, Tooltip("UI prefab representing a single perk option.")]
    [SerializeField] private GameObject perkPrefab;

    [BoxGroup("Perk Manager Setup"), Required, Tooltip("Parent object that will contain instantiated perk UI elements.")]
    [SerializeField] private GameObject perkSelectionUI;

    [BoxGroup("Perk Manager Setup"), Required, Tooltip("List of all available perks.")]
    [SerializeField] private List<PerkSO> perks;

    [BoxGroup("Settings"), Tooltip("Delay before hiding the selected perk.")]
    [SerializeField] private float hideDelay = 3f;

    private readonly List<GameObject> activePerks = new List<GameObject>();
    private Perk selectedPerk;
    private bool perkConfirmed;

    /// <summary>
    /// Clears current perks and generates three new random ones.
    /// </summary>
    [Button, Tooltip("Clears current perks and generates three new random ones.")]
    public void RandomizeNewPerks()
    {
        ClearPerks();

        for (int i = 0; i < 3; i++)
        {
            PerkSO randomPerk = GetRandomPerk();
            GameObject instance = InstantiatePerk(randomPerk);
            activePerks.Add(instance);
        }
    }

    /// <summary>
    /// Instantiates perk prefab and assigns data.
    /// </summary>
    private GameObject InstantiatePerk(PerkSO perkSO)
    {
        GameObject instance = Instantiate(perkPrefab, perkSelectionUI.transform);
        Perk perk = instance.GetComponent<Perk>();

        if (perk == null)
        {
            Debug.LogError("Perk component missing on prefab.");
            return instance;
        }

        perk.Setup(perkSO);
        perk.onPerkSelected += SelectPerk;
        perk.onClickConfirmed += ConfirmPerk;

        return instance;
    }

    /// <summary>
    /// Called when player selects a perk.
    /// </summary>
    private void SelectPerk(Perk perk)
    {
        Debug.Log("hii");    
        selectedPerk = perk;
        Debug.Log($"Player selected perk: {perk}");

        HideUnselectedPerks();
        StartCoroutine(WaitAndHideSelected());
    }

    private void ConfirmPerk()
    {
        perkConfirmed = true;
    }

    /// <summary>
    /// Hides all perks except the selected one.
    /// </summary>
    private void HideUnselectedPerks()
    {
        foreach (GameObject perkObj in activePerks)
        {
            Perk perk = perkObj.GetComponent<Perk>();
            if (perk != selectedPerk)
            {
                perkObj.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Waits and hides the selected perk.
    /// </summary>
    private IEnumerator WaitAndHideSelected()
    {
        yield return new WaitUntil(() => perkConfirmed);
        perkConfirmed = false;
        
        if (selectedPerk != null)
        {
            selectedPerk.gameObject.SetActive(false);
        }
        
        OnPerkSelected.Invoke();
    }

    /// <summary>
    /// Removes all currently active perk instances.
    /// </summary>
    private void ClearPerks()
    {
        foreach (GameObject perk in activePerks)
        {
            if (perk != null)
            {
                Destroy(perk);
            }
        }

        activePerks.Clear();
    }

    /// <summary>
    /// Returns a random perk from the list.
    /// </summary>
    private PerkSO GetRandomPerk()
    {
        if (perks == null || perks.Count == 0)
        {
            Debug.LogError("Perk list is empty.");
            return null;
        }

        return perks[Random.Range(0, perks.Count)];
    }
}