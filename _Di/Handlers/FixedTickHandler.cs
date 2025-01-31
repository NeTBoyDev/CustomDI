using System.Collections.Generic;
using Plugins._Di.Interfaces;
using UnityEngine;

namespace Plugins._Di.Handlers
{
    public class FixedTickHandler : MonoBehaviour
    {
        private readonly HashSet<IFixedTickable> _subscribers = new();
    
        public void AddSubscriber(IFixedTickable subscriber)
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
