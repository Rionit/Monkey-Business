using System;
using UnityEngine;
using UnityEngine.Events;

namespace MonkeyBusiness.Misc
{
    public class StaticEvents : MonoBehaviour
    {
        public static UnityEvent<GameObject> OnItemRegistered = new();
        public static UnityEvent<GameObject> OnItemUnregistered = new();
        public static UnityEvent OnPlayerMeleeAttackUsed = new();
        public static UnityEvent<GameObject> OnEnemyMeleeAttackUsed = new(); // gameobject is enemy using that attack
        public static UnityEvent OnEnemyHit = new();
        public static UnityEvent OnMonksterPicked = new();

        public static void ClearAllEvents()
        {
            OnItemRegistered?.RemoveAllListeners();
            OnItemUnregistered?.RemoveAllListeners();
            OnPlayerMeleeAttackUsed?.RemoveAllListeners();
            OnEnemyMeleeAttackUsed?.RemoveAllListeners();
            OnEnemyHit?.RemoveAllListeners();
            OnMonksterPicked?.RemoveAllListeners();
        }
    }
}
