// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.Projects.FileItems
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false), MeansImplicitUse, BaseTypeRequired(typeof(IFileItemHandler))]
    public class FileItemHandlerAttribute : ExtensibilityAttribute
    {
        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            FileItemManager.LoadType(type, this);
        }
    }
}
