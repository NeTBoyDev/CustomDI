using System.Collections.Generic;
using Plugins._Di.Interfaces;
using UnityEngine;

namespace Plugins._Di.Handlers
{
    public class TickHandler : MonoBehaviour
    {
        private readonly HashSet<ITickable> _subscribers = new();
    
        public void AddSubscriber(ITickable subscriber)
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
