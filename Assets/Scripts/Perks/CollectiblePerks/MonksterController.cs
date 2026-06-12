using System;
using System.Collections;
using MonkeyBusiness.Managers;
using MonkeyBusiness.Misc;
using UnityEngine;

namespace MonkeyBusiness.Items
{
    public class MonksterController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        
        private void Start()
        {
            StaticEvents.OnMonksterPicked.AddListener(OnMonksterPicked);
        }

        private void OnMonksterPicked()
        {
            StatsManager.Instance._equipmentManager.SetCurrentItemHidden(true);
            animator.SetTrigger("MonksterPicked");

            StartCoroutine(UnhideAfterAnimation("monkster_drink"));
        }

        private IEnumerator UnhideAfterAnimation(string stateName)
        {
            yield return null; // wait for animator to enter the state

            while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
                yield return null;

            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                yield return null;

            StatsManager.Instance._equipmentManager.SetCurrentItemHidden(false);
        }
    }
}
