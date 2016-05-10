// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.Commandy.Commands;
using Sitecore.Rocks.UI.Commandy.Modes;

namespace Sitecore.Rocks.UI.Commandy.Searchers
{
    [Export(typeof(ISearcher))]
    public class SetBaseTemplateSearcher : SearcherBase
    {
        public override void Search(CommandyContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var mode = context.Mode as SetBaseTemplateMode;
            if (mode == null)
            {
                return;
            }

            if (!mode.IsReady)
            {
                return;
            }

            var templates = mode.Templates.Where(c => c.Name.IsFilterMatch(context.Text)).ToList();
            if (!templates.Any())
            {
                return;
            }

            var commands = new List<Hit>();

            foreach (var templateHeader in templates)
            {
                var command = new SetBaseTemplate(templateHeader);
                commands.Add(new Hit(templateHeader.Name, command));
            }

            context.Commandy.AddHits(commands);
        }
    }
}
