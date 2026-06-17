using UnityEngine.Events;

namespace MonkeyBusiness.Combat.Regen
{
    public interface IRegen
    {
        public UnityEvent OnCollected { get; }
    }

}