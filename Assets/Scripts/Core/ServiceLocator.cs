using System;
using System.Collections.Generic;

namespace KingSmash.Core
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();
        private static bool _initialized;

        public static void Initialize()
        {
            _services.Clear();
            _initialized = true;
        }

        public static void Register<T>(T implementation) where T : class
        {
            if (!_initialized) throw new InvalidOperationException("ServiceLocator not initialized.");
            var key = typeof(T);
            if (_services.ContainsKey(key))
                GameLogger.Warning("ServiceLocator", $"Overwriting existing service: {key.Name}");
            _services[key] = implementation;
        }

        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;
            throw new InvalidOperationException($"Service not registered: {typeof(T).Name}");
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var obj))
            {
                service = (T)obj;
                return true;
            }
            service = null;
            return false;
        }

        public static void Unregister<T>() where T : class => _services.Remove(typeof(T));
    }
}
