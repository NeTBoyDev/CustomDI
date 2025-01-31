using System;

namespace DI
{
    /// <summary>
    /// Represents a project dependency context
    /// It contains dependencies, which can be accessed at any scene of the project
    /// </summary>
    public abstract class ProjectContext : Context
    {
        private void Awake()
        {
            RegisterDependencies();
            _container.SetProjectDependencies(_registrations);
            _container.Initialize();
        }
    }
}