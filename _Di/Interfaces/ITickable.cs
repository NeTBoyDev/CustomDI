using UnityEngine;

namespace Plugins._Di.Interfaces
{
    /// <summary>
    /// Interface that provides possibility to be subscribed to Update in <b>non</b> <see cref="MonoBehaviour"/> inheritances
    /// </summary>
    public interface ITickable
    {
        public void Tick();
    }
}
