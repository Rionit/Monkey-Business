using System;
using UnityEngine;
using UnityEngine.Events;

namespace MonkeyBusiness.Misc
{
    public class StaticEvents : MonoBehaviour
    {
        public enum CollectiblePerkType
        {
            Monkster,
            Chimpex,
            Crazyape,
            Gorilla
        }

        public static UnityEvent<GameObject> OnItemRegistered = new();
        public static UnityEvent<GameObject> OnItemUnregistered = new();
        public static UnityEvent OnPlayerMeleeAttackUsed = new();
        public static UnityEvent<GameObject> OnEnemyMeleeAttackUsed = new(); // gameobject is enemy using that attack
        public static UnityEvent OnEnemyHit = new();
        public static UnityEvent<float> OnPlayerHeal = new();
        public static UnityEvent<CollectiblePerkType> OnCollectiblePerkPicked = new();
        public static UnityEvent<CollectiblePerkType> OnCollectiblePerkStopped = new();

        public static void ClearAllEvents()
        {
            OnItemRegistered?.RemoveAllListeners();
            OnItemUnregistered?.RemoveAllListeners();
            OnPlayerMeleeAttackUsed?.RemoveAllListeners();
            OnEnemyMeleeAttackUsed?.RemoveAllListeners();
            OnEnemyHit?.RemoveAllListeners();
            OnCollectiblePerkPicked?.RemoveAllListeners();
            OnCollectiblePerkStopped?.RemoveAllListeners();
            OnPlayerHeal?.RemoveAllListeners();
        }
    }
}
