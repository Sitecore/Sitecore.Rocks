// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Chunks.Forms
{
    [Export(typeof(IRenderingChunk))]
    public class SpeakFormRenderingChunk : RenderingChunkBase
    {
        public SpeakFormRenderingChunk()
        {
            Name = "Generate a SPEAK Form Based on an Item";
            Group = "Generators";
        }

        public override void GetRenderings(IRenderingContainer renderingContainer, Action<IEnumerable<RenderingItem>> completed)
        {
            Assert.ArgumentNotNull(renderingContainer, nameof(renderingContainer));
            Assert.ArgumentNotNull(completed, nameof(completed));

            var dialog = new SpeakFormRenderingChunkDialog
            {
                RenderingContainer = renderingContainer,
                DatabaseUri = renderingContainer.DatabaseUri
            };

            if (!dialog.ShowModal())
            {
                completed(Enumerable.Empty<RenderingItem>());
                return;
            }

            var renderingItems = dialog.Renderings;
            if (renderingItems == null)
            {
                completed(Enumerable.Empty<RenderingItem>());
                return;
            }

            completed(renderingItems);
        }
    }
}
