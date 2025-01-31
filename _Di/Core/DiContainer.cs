using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using _Di;
using Plugins._Di.Handlers;
using Plugins._Di.Interfaces;
using UnityEngine;

namespace DI
{
    public class DiContainer 
    {
        private static Dictionary<TypeTagPair, Registration> _projectRegistrations = new();
        private static Dictionary<TypeTagPair, Registration> _sceneRegistrations = new();
        private static readonly HashSet<Type> _resolves = new();

        public static DiContainer Instance
        {
            get
            {
                if (instance == null)
                    instance = new DiContainer();
                return instance;
            }
            private set
            {
                instance = value;
            }
        }

        private static DiContainer instance;

        private TickHandler _tickHandler;
        private LateTickHandler _lateTickHandler;
        private FixedTickHandler _fixedTickHandler;
        
        private DiContainer()
        {
            
        }

        public void SetProjectDependencies(Dictionary<TypeTagPair, Registration> projectRegistrations)
        {
            _projectRegistrations = projectRegistrations;
            _projectRegistrations[new TypeTagPair(){Type = typeof(DiContainer)}] = new Registration
            {
                IsSingleton = true,
                Instance = Instance
            };
        }
        
        public void SetSceneDependencies(Dictionary<TypeTagPair, Registration> sceneRegistrations)
        {
            _sceneRegistrations = sceneRegistrations;
        }

        public void ClearSceneDependencies()
        {
            _sceneRegistrations = new();
        }

        public object Resolve(Type parameter,string tag = null)
        {
            try
            {
                if (_resolves.Contains(parameter))
                {
                    throw new Exception("Cycled dependency detected");
                }
                _resolves.Add(parameter);
                if (tag == null)
                {
                    var reg = _sceneRegistrations.FirstOrDefault((kv) => kv.Key.Type == parameter).Value;
                    
                    if(reg == null)
                        reg = _projectRegistrations.FirstOrDefault((kv) => kv.Key.Type == parameter).Value;
                    
                    if(reg == null)
                        throw new Exception($"Dependency with type {parameter.FullName} was not registered");
                    
                    if (reg.IsSingleton)
                    {
                        if (reg.Instance == null && reg.Factory != null)
                        {
                            reg.Instance = reg.Factory();
                            InjectDependenciesInto(reg.Instance);
                            InjectDependencyInCycle(reg.Instance);
                        }
                            
                        return reg.Instance;
                    }
                    var instance = reg.Factory();
                    InjectDependenciesInto(instance);
                    InjectDependencyInCycle(instance);
                        
                    return instance;
                        
                    
                }
                else
                {
                    var reg = _sceneRegistrations.FirstOrDefault((kv) => kv.Key.Tag == tag).Value;
                    
                    if(reg == null)
                        reg = _projectRegistrations.FirstOrDefault((kv) => kv.Key.Tag == tag).Value;
                    
                    if(reg == null)
                        throw new Exception($"Dependency with type {parameter.FullName} and tag {tag} was not registered");
                    
                    if (reg.IsSingleton)
                    {
                        if (reg.Instance == null && reg.Factory != null)
                        {
                            reg.Instance = reg.Factory();
                            InjectDependenciesInto(reg.Instance);
                            InjectDependencyInCycle(reg.Instance);
                        }
                        return reg.Instance;
                    }
                    var instance = reg.Factory();
                    InjectDependenciesInto(instance);
                    InjectDependencyInCycle(instance);
                    return instance;
                }
                
                
            }
            finally
            {
                _resolves.Remove(parameter);
            }
        }

        private void InjectDependencyInCycle(object obj)
        {
            switch (obj)
            {
                case ITickable tickable:
                    _tickHandler.AddSubscriber(tickable);
                    break;
                case IFixedTickable fixedTickable:
                    _fixedTickHandler.AddSubscriber(fixedTickable);
                    break;
                case ILateTickable lateTickable:
                    _lateTickHandler.AddSubscriber(lateTickable);
                    break;
            }
        }

        private void InjectDependencies()
        {
            var allMonoBehaviours = GameObject.FindObjectsOfType<MonoBehaviour>();

            foreach (var monoBehaviour in allMonoBehaviours)
            {
                InjectDependenciesInto(monoBehaviour);
            }
        }

        private void InjectDependenciesInto(object obj)
        {
            InjectDependenciesIntoMethods(obj);
            InjectDependenciesIntoProperties(obj);
            InjectDependenciesIntoFields(obj);
        }
        
        private void InjectDependenciesIntoMethods(object obj)
        {
            var methods = obj.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttributes(typeof(InjectAttribute), true).Any());
            
            foreach (var method in methods)
            {
                var parameters = method.GetParameters()
                    .Select(p => Resolve(p.ParameterType))
                    .ToArray();

                method.Invoke(obj, parameters);
            }
        }
        
        private void InjectDependenciesIntoFields(object obj)
        {
            var fields = obj.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p => p.GetCustomAttributes(typeof(InjectAttribute), true).Any());
            
            
            
            
            foreach (var field in fields)
            {
                var tag = (field.GetCustomAttribute(typeof(InjectAttribute)) as InjectAttribute).Tag;
                var dependency = Resolve(field.FieldType,tag);
                field.SetValue(obj, dependency);
            }
        }
        private void InjectDependenciesIntoProperties(object obj)
        {
            var properties = obj.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p => p.GetCustomAttributes(typeof(InjectAttribute), true).Any());
            foreach (var property in properties)
            {
                if (property.CanWrite)
                {
                    var tag = (property.GetCustomAttribute(typeof(InjectAttribute)) as InjectAttribute).Tag;
                    var dependency = Resolve(property.PropertyType,tag);
                    property.SetValue(obj, dependency);
                }
            }
           
        }

        public void Initialize()
        {
            GameObject handlerObject = new GameObject("Handler");
            
            _tickHandler = handlerObject.AddComponent<TickHandler>();
            _lateTickHandler = handlerObject.AddComponent<LateTickHandler>();
            _fixedTickHandler = handlerObject.AddComponent<FixedTickHandler>();
            
            InjectDependencies();
        }

        public static UnityEngine.Object Instantiate(UnityEngine.Object original)
        {
            var obj = GameObject.Instantiate(original);
            Instance.InjectDependenciesInto(obj);
            return obj;
        }
        
        public static UnityEngine.Object Instantiate(UnityEngine.Object original,Vector3 pos,Quaternion rotation)
        {
            var obj = GameObject.Instantiate(original,pos,rotation);
            Instance.InjectDependenciesInto(obj);
            return obj;
        }
        
        public static T Instantiate<T>(T original) where T : Component
        {
            var obj = GameObject.Instantiate(original);
            Instance.InjectDependenciesInto(obj);
            return obj;
        }
        
        public static T Instantiate<T>(string name) where T : Component
        {
            var obj = new GameObject(name).AddComponent<T>();
            Instance.InjectDependenciesInto(obj);
            return obj;
        }
        
        public static T Instantiate<T>(T original,Vector3 pos,Quaternion rotation) where T : Component
        {
            var obj = GameObject.Instantiate(original,pos,rotation);
            Instance.InjectDependenciesInto(obj);
            return obj;
        }
        
    }

   

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, Inherited = true)]
    public class InjectAttribute : Attribute
    {
        public InjectAttribute(string tag)
        {
            Tag = tag;
        }

        public InjectAttribute()
        {
            
        }
        public string Tag { get; set; } = null;
    }

    public class TypeTagPair
    {
        public Type Type;
        public string Tag;
    }
}
