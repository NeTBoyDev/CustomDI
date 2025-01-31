# Dependency Injection Context for Unity

## Overview
This project provides a lightweight Dependency Injection (DI) system for Unity, allowing flexible and structured management of dependencies within your game or application.

## Features
- Register dependencies by type, instance, factory, or scene object.
- Supports singleton and transient lifecycle management.
- Allows registration of MonoBehaviour dependencies.
- Resolves dependencies via constructor injection.

## Installation
1. Add the `DI` namespace to your project.
2. Ensure that your scripts extend the `Context` class and override `RegisterDependencies()`.

## Usage

### Registering Dependencies
```csharp
public class GameContext : Context
{
    public override void RegisterDependencies()
    {
        Register<IPlayer, Player>(isSingleton: true);
        RegisterFromInstance(new GameSettings());
        RegisterFactory<IEnemy>(() => new Enemy());
        RegisterFromScene<UIManager>();
    }
}
```

### Resolving Dependencies
Dependencies can be resolved using the `DiContainer.Instance.Resolve<T>()` method.
```csharp
var player = DiContainer.Instance.Resolve<IPlayer>();
```

## API Reference

### `Register<TService, TImplementation>(bool isSingleton)`
Registers a dependency by interface-to-implementation mapping.
- `isSingleton`: If `true`, a single instance is maintained; otherwise, a new instance is created on each resolve.

### `Register<TService>(bool isSingleton)`
Registers a dependency by its type without specifying an implementation.

### `RegisterFromInstance<TService>(TService instance)`
Registers an existing instance as a singleton dependency.

### `RegisterFactory<TService>(Func<object> factory)`
Registers a dependency using a custom factory function.

### `RegisterFromScene<TService>()`
Finds an object of type `TService` in the scene hierarchy and registers it as a singleton.

### `RegisterFromScene<TService>(string name)`
Finds a named object in the scene and registers its component as a singleton.

### `RegisterMonoBehavior<TService>(TService prefab, bool isSingleton)`
Registers a `MonoBehaviour` dependency from a prefab.

### `RegisterWithParameters<TService, TImplementation>(bool isSingleton, params object[] parameters)`
Registers a dependency and allows passing constructor parameters.

## Error Handling
- If a dependency cannot be found in the scene, an exception is thrown.
- If no suitable constructor is found during resolution, an exception is thrown.

## Logging
Debug logs are used to track registrations:
```csharp
Debug.Log($"Registered {typeof(TService).FullName} as singleton");
```

## License
MIT License. Free to use and modify.

