// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell.Environment;

namespace Sitecore.Rocks.ContentTrees.StatusIcons
{
    public static class EnvHostExtensions
    {
        static EnvHostExtensions()
        {
            AppHost.Container.Register<StatusIconHost, StatusIconHost>().AsSingleton();
        }

        [NotNull]
        public static StatusIconHost StatusIcons([NotNull] this EnvHost envHost)
        {
            Assert.ArgumentNotNull(envHost, nameof(envHost));

            return AppHost.Container.Resolve<StatusIconHost>();
        }
    }
}
