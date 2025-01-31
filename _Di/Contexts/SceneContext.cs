using System;

namespace DI
{
    /// <summary>
    /// Represents a scene dependency context
    /// It contains dependencies, which can be accessed only at the current scene of the project 
    /// </summary>
    public abstract class SceneContext : Context
    {
        private void Awake()
        {
            RegisterDependencies();
            _container.SetSceneDependencies(_registrations);
            _container.Initialize();
        }
    }
}