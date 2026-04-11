using UnityEngine;

namespace MonkeyBusiness.Misc
{
    /// <summary>
    /// Interface for objects that can be hit by attacks that provides the desirable target object.
    /// </summary>
    /// <remarks><i>Used to remove Player<-Combat dependency (Player->Combat dependency will most likely be used for weapons). </i></remarks>
    public interface ITargetable
    {
        public GameObject Target { get; }
    }
}