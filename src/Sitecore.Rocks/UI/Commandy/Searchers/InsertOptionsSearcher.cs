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
    public class InsertOptionsSearcher : SearcherBase
    {
        public override void Search(CommandyContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var mode = context.Mode as InsertMode;
            if (mode == null)
            {
                return;
            }

            if (!mode.IsReady)
            {
                return;
            }

            var templates = mode.InsertOptions.Where(c => c.Name.IsFilterMatch(context.Text)).ToList();
            if (!templates.Any())
            {
                return;
            }

            var commands = new List<Hit>();

            foreach (var itemHeader in templates)
            {
                commands.Add(new Hit(itemHeader.Name, itemHeader));
            }

            context.Commandy.AddHits(commands);
        }
    }
}
