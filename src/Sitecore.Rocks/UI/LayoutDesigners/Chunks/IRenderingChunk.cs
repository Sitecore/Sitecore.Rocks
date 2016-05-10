// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Chunks
{
    public interface IRenderingChunk
    {
        [NotNull]
        string Group { get; }

        [NotNull]
        string Name { get; }

        void GetRenderings([NotNull] IRenderingContainer renderingContainer, [NotNull] Action<IEnumerable<RenderingItem>> completed);
    }
}
