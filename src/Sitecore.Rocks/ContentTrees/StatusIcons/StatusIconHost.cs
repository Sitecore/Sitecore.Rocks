// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Extensibility.Composition;

namespace Sitecore.Rocks.ContentTrees.StatusIcons
{
    public class StatusIconHost
    {
        public StatusIconHost()
        {
            AppHost.Extensibility.ComposeParts(this);

            AppHost.Extensibility.Reset += () => AppHost.Extensibility.ComposeParts(this);
        }

        [NotNull, ImportMany(typeof(IStatusIcon))]
        public IEnumerable<IStatusIcon> StatusIcons { get; set; }
    }
}
