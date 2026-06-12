using MonkeyBusiness.Managers;

namespace MonkeyBusiness.Combat.Regen
{
    public interface IAmmoRegen : IRegen
    {
        public void RestoreAmmo(EquipmentManager equipmentManager, float percentage)
        {
            foreach(var item in equipmentManager.Items)
            {
                if(item is IWeapon weapon)
                {
                    weapon.ReloadPercent(percentage);
                }
            }
            OnCollected?.Invoke();
        }
    }
}