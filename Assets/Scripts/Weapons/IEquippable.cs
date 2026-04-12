using UnityEngine;

namespace MonkeyBusiness.Weapons
{
    public interface IEquippable
    {
        public void Equip();
        public void Unequip();
        public void Use();
    }
}
