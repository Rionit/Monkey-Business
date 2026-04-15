using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace MonkeyBusiness.Misc
{
    public interface IEquippable
    {
        public bool IsEquipped {get;}
        public void Equip();

        public UnityEvent<IEquippable> OnEquipped { get; }

        public UnityEvent<IEquippable> OnUnequipped { get; }

        public void Unequip();
        public void Use();
    }
}
