// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Linq;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.Commandy.Modes;

namespace Sitecore.Rocks.UI.Commandy.Searchers
{
    [Export(typeof(ISearcher))]
    public class CommandSearcher : SearcherBase
    {
        public override void Search(CommandyContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var mode = context.Mode as CommandMode;
            if (mode == null)
            {
                return;
            }

            var commands = mode.Commands.Where(c => c.Text.IsFilterMatch(context.Text)).ToList();
            if (!commands.Any())
            {
                return;
            }

            context.Commandy.AddHits(commands.Select(c => new Hit(c.Text, c)));
        }
    }
}
