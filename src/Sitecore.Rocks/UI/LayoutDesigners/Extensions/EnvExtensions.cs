// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Shell.Environment;

namespace Sitecore.Rocks.UI.LayoutDesigners.Extensions
{
    public static class EnvExtensions
    {
        static EnvExtensions()
        {
            AppHost.Container.Register<LayoutDesignerHost, LayoutDesignerHost>().AsSingleton();
        }

        [NotNull]
        public static LayoutDesignerHost LayoutDesigner([NotNull] this EnvHost envHost)
        {
            Assert.ArgumentNotNull(envHost, nameof(envHost));

            return AppHost.Container.Resolve<LayoutDesignerHost>();
        }
    }
}
