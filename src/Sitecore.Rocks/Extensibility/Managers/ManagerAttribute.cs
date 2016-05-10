// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Extensibility.Managers
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false), MeansImplicitUse]
    public class ManagerAttribute : ExtensibilityAttribute
    {
        public override void PostInitialize(Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            if (type.IsAbstract)
            {
                return;
            }

            AppHost.Container.Register(type).AsSingleton();

            var manager = AppHost.Container.Resolve(type) as IManager;
            if (manager != null)
            {
                manager.Initialize();
            }
        }
    }
}
