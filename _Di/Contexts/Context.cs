using System;
using System.Collections.Generic;
using System.Linq;
using _Di;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DI
{
    [DefaultExecutionOrder(-10)]
    public abstract class Context : MonoBehaviour
    {
        public abstract void RegisterDependencies();

        protected DiContainer _container = DiContainer.Instance;

        protected readonly Dictionary<TypeTagPair, Registration> _registrations = new();

        /// <summary>
        /// Registering your dependency by Base -> Inheritor relation
        /// Returns the implementation by the base type
        /// </summary>
        /// <param name="isSingleton">The dependency life cycle
        /// <para>If <b>true</b> - will return the only one instance of an object</para>
        /// If <b>false</b> - will create a new instance of the object for any resolve</param>
        /// <typeparam name="TService">The base type</typeparam>
        /// <typeparam name="TImplementation">The implementation type</typeparam>
        public TypeTagPair Register<TService, TImplementation>(bool isSingleton) where TImplementation : TService
        {
            var registration = new Registration
            {
                Factory = () => CreateInstance<TImplementation>(),
                IsSingleton = isSingleton
            };

            var pair = new TypeTagPair() { Type = typeof(TService) };
            _registrations[pair] = registration;

            Debug.Log($"Registered {typeof(TService).FullName} as {(isSingleton ? "singleton" : "transient")}");

            return pair;
        }
        
        /// <summary>
        /// Registering your dependency by its type
        /// </summary>
        /// <param name="isSingleton">The dependency life cycle
        /// <para>If <b>true</b> - will return the only one instance of an object</para>
        /// If <b>false</b> - will create a new instance of the object for any resolve</param>
        /// <typeparam name="TService">The dependency type</typeparam>
        public TypeTagPair Register<TService>(bool isSingleton)
        {
            var registration = new Registration
            {
                Factory = () => CreateInstance<TService>(),
                IsSingleton = isSingleton
            };

            var pair = new TypeTagPair() { Type = typeof(TService) };
            _registrations[pair] = registration;

            Debug.Log($"Registered {typeof(TService).FullName} as {(isSingleton ? "singleton" : "transient")}");

            return pair;
        }

        /// <summary>
        /// Registering your dependency by its instance
        /// This means that you will resolve an object instance, which you pass as the instance parameter
        /// </summary>
        /// <param name="instance">Instance of your dependency</param>
        /// <typeparam name="TService">The dependency type</typeparam>
        public TypeTagPair RegisterFromInstance<TService>(TService instance)
        {
            var registration = new Registration
            {
                Factory = () => instance,
                IsSingleton = true,
                Instance = instance
            };
            var pair = new TypeTagPair() { Type = typeof(TService) };
            _registrations[pair] = registration;
            
            Debug.Log($"Registered instance of {typeof(TService).FullName}");

            return pair;
        }

        /// <summary>
        /// Registering the factory, which will produce instances of your dependency
        /// </summary>
        /// <param name="factory">The producing logic delegate. Here you can describe the producing rules. At the end, you should return your object</param>
        /// <typeparam name="TService">The dependency type</typeparam>
        public TypeTagPair RegisterFactory<TService>(Func<object> factory)
        {
            var registration = new Registration
            {
                Factory = factory ,
                IsSingleton = false,
                Instance = null
            };
            var pair = new TypeTagPair() { Type = typeof(TService) };
            _registrations[pair] = registration;
            
            Debug.Log($"Registered instance of {typeof(TService).FullName}");

            return pair;
        }
        
        /// <summary>
        /// Registering your dependency by searching it at the scene hierarchy. 
        /// </summary>
        /// <typeparam name="TService">The dependency type</typeparam>
        /// <exception cref="Exception">Throws the exception when can't find the dependency at the current scene hierarchy</exception>
        public TypeTagPair RegisterFromScene<TService>() where TService : Object
        {
            var instance = FindObjectOfType<TService>();
            if (instance == null)
            {
                throw new Exception($"Instance of type {typeof(TService).FullName} not found in scene");
            }
            
            var registration = new Registration
            {
                Factory = () => instance,
                IsSingleton = true,
                Instance = instance
            };

            var pair = new TypeTagPair() { Type = typeof(TService) };
            _registrations[pair] = registration;
            Debug.Log($"Registered instance of {typeof(TService).FullName}");

            return pair;
        }
        
        /// <summary>
        /// Registering your dependency by searching it at the scene hierarchy by name. 
        /// </summary>
        /// <typeparam name="TService">The dependency type</typeparam>
        /// <exception cref="Exception">Throws the exception when can't find the dependency at the current scene hierarchy</exception>
        /// <param name="name">The name of the object in the scene hierarchy</param>
        public TypeTagPair RegisterFromScene<TService>(string name) where TService : Object
        {
            var instance = GameObject.Find(name).GetComponent<TService>();
            if (instance == null)
            {
                throw new Exception($"Instance of type {typeof(TService).FullName} not found in scene");
            }
            
            var registration = new Registration
            {
                Factory = () => instance,
                IsSingleton = true,
                Instance = instance
            };

            var pair = new TypeTagPair() { Type = typeof(TService) };
            _registrations[pair] = registration;
            Debug.Log($"Registered instance of {typeof(TService).FullName}");

            return pair;
        }
        
        /// <summary>
        /// Registering your dependency, which should be inherited from <see cref="MonoBehaviour"/>
        /// </summary>
        /// <param name="prefab">The instance of dependency object</param>
        /// <param name="isSingleton">The dependency life cycle
        /// <para>If <b>true</b> - will return the only one instance of an object</para>
        /// If <b>false</b> - will create a new instance of the object for any resolve</param>
        /// <typeparam name="TService">The dependency type</typeparam>
        /// <exception cref="ArgumentNullException">The exception which will be throw if your prefab is <c>null</c></exception>
        public TypeTagPair RegisterMonoBehavior<TService>(TService prefab, bool isSingleton) where TService : MonoBehaviour
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab), "Prefab cannot be null");
            }
            
            var registration = new Registration
            {
                Factory = () => DiContainer.Instantiate(prefab),
                IsSingleton = isSingleton,
                Instance = isSingleton ? prefab : null
            };
            var pair = new TypeTagPair() { Type = typeof(TService) };
            _registrations[pair] = registration;

            return pair;
        }
            
        
        protected object CreateInstance<T>()
        {
            var constructors = typeof(T).GetConstructors();
            
            
            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                var parameterInstances = parameters.Select(p =>_container.Resolve(p.ParameterType)).ToArray();

                return constructor.Invoke(parameterInstances);
            }

            return Activator.CreateInstance<T>();
            throw new Exception($"No suitable constructor found for type {typeof(T).FullName}");
        }

        /// <summary>
        /// Registering your dependency with the params of the parameters, which will be passed into it at the resolving process
        /// </summary>
        /// <param name="isSingleton">The dependency life cycle
        /// <para>If <b>true</b> - will return the only one instance of an object</para>
        /// If <b>false</b> - will create a new instance of the object for any resolve</param>
        /// <param name="parameters">List of the parameters, which will be passed into dependency at the resolving process</param>
        /// <typeparam name="TService">The base type</typeparam>
        /// <typeparam name="TImplementation">The implementation type</typeparam>
        public TypeTagPair RegisterWithParameters<TService, TImplementation>(bool isSingleton, params object[] parameters) where TImplementation : TService
        {
            var registration = new Registration
            {
                Factory = () => CreateInstanceWithParameters<TImplementation>(parameters),
                IsSingleton = isSingleton
            };

            var pair = new TypeTagPair() { Type = typeof(TImplementation) };
            _registrations[pair] = registration;

            Debug.Log($"Registered {typeof(TService).FullName} with parameters as {(isSingleton ? "singleton" : "transient")}");

            return pair;
        }
        
        /// <summary>
        /// Registering your dependency with the params of the parameters, which will be passed into it at the resolving process
        /// </summary>
        /// <param name="isSingleton">The dependency life cycle
        /// <para>If <b>true</b> - will return the only one instance of an object</para>
        /// If <b>false</b> - will create a new instance of the object for any resolve</param>
        /// <param name="parameters">List of the parameters, which will be passed into dependency at the resolving process</param>
        /// <typeparam name="TImplementation">The dependency type</typeparam>
        public TypeTagPair RegisterWithParameters<TImplementation>(bool isSingleton, params object[] parameters)
        {
            var registration = new Registration
            {
                Factory = () => CreateInstanceWithParameters<TImplementation>(parameters),
                IsSingleton = isSingleton
            };
            var pair = new TypeTagPair() { Type = typeof(TImplementation) };
            _registrations[pair] = registration;

            Debug.Log($"Registered {typeof(TImplementation).FullName} with parameters as {(isSingleton ? "singleton" : "transient")}");

            return pair;
        }
       

        private object CreateInstanceWithParameters<T>(object[] parameters)
        {
            var constructors = typeof(T).GetConstructors();

            foreach (var constructor in constructors)
            {
                var ctorParams = constructor.GetParameters();
                var resolvedParams = new object[ctorParams.Length];

                for (int i = 0; i < ctorParams.Length; i++)
                {
                    var paramType = ctorParams[i].ParameterType;

                    var matchingParam = parameters.FirstOrDefault(p => paramType.IsInstanceOfType(p));
                    if (matchingParam != null)
                    {
                        resolvedParams[i] = matchingParam;
                    }
                    else
                    {
                        resolvedParams[i] = _container.Resolve(paramType);
                    }
                }

                if (resolvedParams.All(r => r != null))
                {
                    return constructor.Invoke(resolvedParams);
                }
            }

            throw new Exception($"No suitable constructor found for type {typeof(T).FullName} with the given parameters or resolvable dependencies");
        }
        
    }
}