// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.Commandy.Modes;

namespace Sitecore.Rocks.UI.Commandy.Searchers
{
    [Export(typeof(ISearcher))]
    public class InsertRenderingSearcher : SearcherBase
    {
        public override void Search(CommandyContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var mode = context.Mode as InsertRenderingMode;
            if (mode == null)
            {
                return;
            }

            if (!mode.IsReady)
            {
                return;
            }

            var renderingItems = mode.RenderingItems.Where(c => c.Name.IsFilterMatch(context.Text)).ToList();
            if (!renderingItems.Any())
            {
                return;
            }

            var commands = new List<Hit>();

            foreach (var renderingItem in renderingItems)
            {
                commands.Add(new Hit(renderingItem.Name, renderingItem));
            }

            context.Commandy.AddHits(commands);
        }

        public override void SetActiveMode(IMode mode)
        {
            Assert.ArgumentNotNull(mode, nameof(mode));

            var insertRenderingMode = mode as InsertRenderingMode;
            if (insertRenderingMode != null)
            {
                insertRenderingMode.Load();
            }
        }
    }
}
