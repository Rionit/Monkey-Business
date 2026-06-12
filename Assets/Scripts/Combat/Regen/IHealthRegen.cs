using MonkeyBusiness.Combat.Health;

namespace MonkeyBusiness.Combat.Regen
{
    public interface IHealthRegen : IRegen
    {
        public void RestoreHealth(HealthController healthController, float amount)
        {
            healthController.Heal(amount);
            OnCollected?.Invoke();
        }
    }
}