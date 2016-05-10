// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensibility
{
    public class Container
    {
        protected readonly Dictionary<Type, string> ServiceNames = new Dictionary<Type, string>();

        protected readonly Dictionary<string, Func<object>> Services = new Dictionary<string, Func<object>>();

        [NotNull]
        public T Get<T>() where T : class, new()
        {
            string serviceName;
            return ServiceNames.TryGetValue(typeof(T), out serviceName) ? Resolve<T>(serviceName) : new T();
        }

        [NotNull]
        public Registration Register<TS, TC>() where TC : TS
        {
            return Register<TS, TC>(typeof(TS).FullName);
        }

        [NotNull]
        public Registration Register<TS, TC>([NotNull] string name) where TC : TS
        {
            Assert.ArgumentNotNull(name, nameof(name));

            if (!ServiceNames.ContainsKey(typeof(TS)))
            {
                ServiceNames[typeof(TS)] = name;
            }

            return new Registration(this, name, typeof(TC));
        }

        [NotNull]
        public Registration Register([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            if (!ServiceNames.ContainsKey(type))
            {
                ServiceNames[type] = type.FullName;
            }

            return new Registration(this, type.FullName, type);
        }

        [NotNull]
        public T Resolve<T>([NotNull] string name) where T : class
        {
            Assert.ArgumentNotNull(name, nameof(name));

            return (T)Services[name]();
        }

        [NotNull]
        public T Resolve<T>() where T : class
        {
            return Resolve<T>(ServiceNames[typeof(T)]);
        }

        [CanBeNull]
        public object Resolve([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            return Services[ServiceNames[type]]();
        }

        public class Registration
        {
            private readonly Dictionary<string, Func<object>> _args;

            private readonly Container _container;

            private readonly string _name;

            internal Registration([NotNull] Container container, [NotNull] string name, [NotNull] Type type)
            {
                Debug.ArgumentNotNull(container, nameof(container));
                Debug.ArgumentNotNull(name, nameof(name));
                Debug.ArgumentNotNull(type, nameof(type));

                _container = container;
                _name = name;

                var c = type.GetConstructors().First();
                _args = c.GetParameters().ToDictionary<ParameterInfo, string, Func<object>>(x => x.Name, x => (() => container.Services[container.ServiceNames[x.ParameterType]]()));

                container.Services[name] = () => c.Invoke(_args.Values.Select(x => x()).ToArray());
            }

            [NotNull]
            public Registration AsSingleton()
            {
                var service = _container.Services[_name];

                object value = null;
                _container.Services[_name] = () => value ?? (value = service());

                return this;
            }

            [NotNull]
            public Registration WithDependency([NotNull] string parameter, [NotNull] string component)
            {
                Assert.ArgumentNotNull(parameter, nameof(parameter));
                Assert.ArgumentNotNull(component, nameof(component));

                _args[parameter] = () => _container.Services[component]();

                return this;
            }

            [NotNull]
            public Registration WithValue([NotNull] string parameter, [NotNull] object value)
            {
                Assert.ArgumentNotNull(parameter, nameof(parameter));
                Assert.ArgumentNotNull(value, nameof(value));

                _args[parameter] = () => value;

                return this;
            }
        }
    }
}
