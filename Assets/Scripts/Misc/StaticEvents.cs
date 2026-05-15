using System;
using UnityEngine;

namespace MonkeyBusiness.Misc
{
    public class StaticEvents : MonoBehaviour
    {
        public static Action<GameObject> OnItemRegistered;
        public static Action<GameObject> OnItemUnregistered;
    }
}
