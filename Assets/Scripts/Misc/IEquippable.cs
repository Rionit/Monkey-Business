using UnityEngine;

namespace MonkeyBusiness.Misc
{
    public interface IEquippable
    {
        public bool IsEquipped {get;}
        public void Equip();
        public void Unequip();
        public void Use();
    }
}
