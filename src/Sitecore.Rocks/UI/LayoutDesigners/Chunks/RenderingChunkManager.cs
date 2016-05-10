// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Extensibility.Managers;

namespace Sitecore.Rocks.UI.LayoutDesigners.Chunks
{
    public class RenderingChunkManager : ComposableManagerBase<IRenderingChunk>
    {
        [NotNull]
        public IEnumerable<IRenderingChunk> RenderingChunks => Items;
    }
}
