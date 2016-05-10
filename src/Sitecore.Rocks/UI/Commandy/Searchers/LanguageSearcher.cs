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
    public class LanguageSearcher : SearcherBase
    {
        public override void Search(CommandyContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var mode = context.Mode as LanguageMode;
            if (mode == null)
            {
                return;
            }

            if (!mode.IsReady)
            {
                return;
            }

            var languages = mode.Languages.Where(c => c.Name.IsFilterMatch(context.Text) || c.DisplayName.IsFilterMatch(context.Text));
            if (!languages.Any())
            {
                return;
            }

            var commands = new List<Hit>();

            foreach (var language in languages)
            {
                commands.Add(new Hit(language.Name + " - " + language.DisplayName, language));
            }

            context.Commandy.AddHits(commands);
        }
    }
}
