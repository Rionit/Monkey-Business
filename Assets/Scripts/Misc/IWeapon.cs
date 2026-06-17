

using MonkeyBusiness.Misc;
using UnityEngine.Events;

public interface IWeapon : IEquippable
{
    public bool HasAmmo { get; }
    public int CurrentAmmo { get; }

    public int MaxAmmo { get; }

    public void Reload(int ammo);
    public void ReloadPercent(float percentage);

    public UnityEvent<IWeapon> OnAmmoChanged { get; }

}