// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using System.Reflection;
using System.Web.Routing;
using Sitecore.Diagnostics;

namespace Sitecore.Rocks.Server.Extensibility.Composition
{
    public class ExportDefinition
    {
        private ConstructorInfo constructorInfo;

        public ExportDefinition([NotNull] ExportAttribute attribute, [NotNull] Type type)
        {
            Assert.ArgumentNotNull(attribute, nameof(attribute));
            Assert.ArgumentNotNull(type, nameof(type));

            Attribute = attribute;
            Type = type;
        }

        [NotNull]
        public ExportAttribute Attribute { get; private set; }

        public CreationPolicy CreationPolicy { get; set; }

        [NotNull]
        public Type Type { get; }

        [CanBeNull]
        protected object Instance { get; set; }

        [CanBeNull]
        public object GetInstance([CanBeNull] RouteValueDictionary parameters = null, CreationPolicy creationPolicy = CreationPolicy.Any)
        {
            var policy = CreationPolicy.Shared;
            if (parameters != null || creationPolicy == CreationPolicy.NonShared || CreationPolicy == CreationPolicy.NonShared)
            {
                policy = CreationPolicy.NonShared;
            }

            if (policy == CreationPolicy.Shared && Instance != null)
            {
                return Instance;
            }

            if (constructorInfo == null)
            {
                // TODO: make this better
                constructorInfo = Type.GetConstructors().First();
            }

            object[] values = null;
            if (parameters != null)
            {
                values = constructorInfo.GetParameters().Select(parameterInfo => parameters[parameterInfo.Name]).ToArray();
            }

            var instance = constructorInfo.Invoke(values);

            if (policy == CreationPolicy.Shared)
            {
                Instance = instance;
            }

            return instance;
        }

        [CanBeNull]
        public T GetInstance<T>([CanBeNull] RouteValueDictionary parameters = null, CreationPolicy creationPolicy = CreationPolicy.Any) where T : class, new()
        {
            return GetInstance(parameters, creationPolicy) as T;
        }
    }
}
