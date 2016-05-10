// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Chunks
{
    public abstract class RenderingChunkBase : IRenderingChunk
    {
        public string Group { get; protected set; }

        public string Name { get; protected set; }

        public abstract void GetRenderings(IRenderingContainer renderingContainer, Action<IEnumerable<RenderingItem>> completed);
    }
}
