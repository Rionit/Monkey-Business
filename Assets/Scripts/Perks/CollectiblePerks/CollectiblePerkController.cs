using System;
using System.Collections;
using System.Collections.Generic;
using Ami.BroAudio;
using MonkeyBusiness.Managers;
using MonkeyBusiness.Misc;
using UnityEngine;

namespace MonkeyBusiness.Items
{
    public abstract class CollectiblePerkController : MonoBehaviour
    {
        public static readonly Dictionary<StaticEvents.CollectiblePerkType, bool> ActivePerks = new();
        public static bool isAnimating;
        
        [SerializeField] private StaticEvents.CollectiblePerkType perkType;

        [SerializeField] protected GameObject drinkCan;
        
        protected Animator animator;

        protected Coroutine activeRoutine;

        private void Start()
        {
            ActivePerks.TryAdd(perkType, false);
            
            StaticEvents.OnCollectiblePerkPicked.AddListener(OnPerkPicked);
            animator = GetComponentInChildren<Animator>();
        }
        
        public static bool IsActive(StaticEvents.CollectiblePerkType perkType)
        {
            return ActivePerks.TryGetValue(perkType, out bool active) && active;
        }
        
        private void OnPerkPicked(StaticEvents.CollectiblePerkType pickedType)
        {
            if (pickedType != perkType)
                return;

            Activate();
        }

        protected void Activate()
        {
            StatsManager.Instance._equipmentManager.CanReceiveInput = false;
            StatsManager.Instance._equipmentManager.SetCurrentItemHidden(true);
            drinkCan.SetActive(true);

            GetComponent<SoundSource>().Play();

            isAnimating = true;
            animator.SetTrigger("MonksterPicked");
            StartCoroutine(UnhideAfterAnimation("monkster_drink"));
            
            if (activeRoutine != null)
                StopCoroutine(activeRoutine);

            activeRoutine = StartCoroutine(RunPerk());
        }

        private IEnumerator RunPerk()
        {
            ActivePerks[perkType] = true;

            ApplyEffect();

            float duration = GetDuration();
            yield return new WaitForSeconds(duration);

            ResetEffect();

            activeRoutine = null;
            ActivePerks[perkType] = false;
            StaticEvents.OnCollectiblePerkStopped.Invoke(perkType);
        }
        
        private IEnumerator UnhideAfterAnimation(string stateName)
        {
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
                yield return null;

            while (animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
                yield return null;

            StatsManager.Instance._equipmentManager.SetCurrentItemHidden(false);
            StatsManager.Instance._equipmentManager.CanReceiveInput = true;
            drinkCan.SetActive(false);
            isAnimating = false;
        }

        protected abstract void ApplyEffect();
        protected abstract void ResetEffect();
        protected abstract float GetDuration();
    }
}