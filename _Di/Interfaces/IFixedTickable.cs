using UnityEngine;

namespace Plugins._Di.Interfaces
{
    /// <summary>
    /// Interface that provides possibility to be subscribed to FixedUpdate in <b>non</b> <see cref="MonoBehaviour"/> inheritances
    /// </summary>
    public interface IFixedTickable
    {
        public void Tick();
    }
}
