using System.Collections.Generic;
using Plugins._Di.Interfaces;
using UnityEngine;

namespace Plugins._Di.Handlers
{
    public class LateTickHandler : MonoBehaviour
    {
        private readonly HashSet<ILateTickable> _subscribers = new();
    
        public void AddSubscriber(ILateTickable subscriber)
        {
            _subscribers.Add(subscriber);
        }

        private void Update()
        {
            foreach (var subscriber in _subscribers)
            {
                subscriber.Tick();
            }
        }
    }
}
