// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Pipelines;

namespace Sitecore.Rocks.Shell.Pipelines.SetResources
{
    public class SetResourcePipeline : Pipeline<SetResourcePipeline>
    {
        [NotNull]
        public FrameworkElement FrameworkElement { get; private set; }

        public void WithParameters([NotNull] FrameworkElement frameworkElement)
        {
            Assert.ArgumentNotNull(frameworkElement, nameof(frameworkElement));

            FrameworkElement = frameworkElement;

            Start();
        }
    }
}
