// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility;

namespace Sitecore.Rocks.ContentTrees.Commands.Sorting.Files
{
    [MeansImplicitUse]
    public class FileSortAttribute : ExtensibilityAttribute
    {
        public FileSortAttribute([NotNull] string name)
        {
            Assert.ArgumentNotNull(name, nameof(name));

            Name = name;
        }

        [NotNull]
        public string Name { get; private set; }

        public override void Initialize([NotNull] Type type)
        {
            Assert.ArgumentNotNull(type, nameof(type));

            FileSortManager.LoadType(type, this);
        }
    }
}
