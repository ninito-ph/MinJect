using System;
using System.Collections.Generic;
using System.Reflection;
using Ninito.MinJect.Injection.Exceptions;
using Ninito.MinJect.Reflection;

namespace Ninito.MinJect.Injection
{
    /// <summary>
    /// A class that injects dependencies into injectables
    /// </summary>
    public sealed class Injector : IInjectable
    {
        #region Private Fields

        private readonly Injector _parentInjector;

        private readonly Dictionary<Type, object> _objects = new Dictionary<Type, object>();

        #endregion

        #region Constructors

        public Injector(Injector parent = null)
        {
            _parentInjector = parent;
            Bind(this);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Binds a specific instance to its type on the injector
        /// </summary>
        /// <param name="instance">The instance to bind to the injector</param>
        /// <typeparam name="T">The type of the instance</typeparam>
        public void Bind<T>(T instance) where T : IInjectable
        {
            Type type = typeof(T);
            _objects[type] = instance;
        }

        /// <summary>
        /// Injects dependencies on bindings, in case they depend on other bindings
        /// </summary>
        public void InjectDependenciesOnBindings()
        {
            foreach (object instance in _objects.Values)
            {
                InjectDependenciesOf(instance);
            }
        }

        /// <summary>
        /// Injects all dependencies of the specified instance
        /// </summary>
        /// <param name="instance">The instance to inject dependencies on</param>
        public void InjectDependenciesOf(object instance)
        {
            FieldInfo[] fields = Reflector.GetInjectableFieldsOf(instance.GetType());

            for (int index = 0, max = fields.Length; index < max; index++)
            {
                fields[index].SetValue(instance, GetBindingOf(fields[index].FieldType));
            }
        }
        
        /// <summary>
        /// Gets the instance of the specified binding
        /// </summary>
        /// <typeparam name="T">The key of the binding</typeparam>
        /// <returns>The instance of the specified binding</returns>
        public T GetInstance<T>()
        {
            return (T) GetBindingOf(typeof(T));
        }
        
        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the binding of the given type
        /// </summary>
        /// <param name="bindingKey">The type to get a binding of</param>
        /// <returns></returns>
        /// <exception cref="InjectorException">An exception given when the type can't be found in the injector or
        /// one of its parents</exception>
        private object GetBindingOf(Type bindingKey)
        {
            if (_objects.TryGetValue(bindingKey, out object obj)) return obj;

            if (HasParentInjector())
            {
                return GetBindingFromParent(bindingKey);
            }

            throw new InjectorException("Could not get " + bindingKey.FullName + " from injector");
        }

        /// <summary>
        /// Checks whether the injector has a parent
        /// </summary>
        /// <returns>Whether the injector has a parent</returns>
        private bool HasParentInjector()
        {
            return _parentInjector != null;
        }

        /// <summary>
        /// Gets the binding for the specified type from the parent injector
        /// </summary>
        /// <param name="bindingKey">The key the dependency was bound to</param>
        /// <returns>The binding of the given type from the parent</returns>
        private object GetBindingFromParent(Type bindingKey)
        {
            return _parentInjector.GetBindingOf(bindingKey);
        }

        #endregion
    }
}