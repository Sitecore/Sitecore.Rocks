// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Shell.ComponentModel;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners
{
    public interface IRenderingContainer
    {
        [NotNull]
        DatabaseUri DatabaseUri { get; }

        [NotNull]
        string Layout { get; }

        [NotNull]
        IEnumerable<RenderingItem> Renderings { get; }

        void GetDataBindingValues([NotNull] RenderingItem renderingItem, [NotNull] DynamicProperty dynamicProperty, [NotNull] List<string> values);
    }
}
