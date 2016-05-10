// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensibility.Composition;
using Sitecore.Rocks.Extensions.DataServiceExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.Commandy.Modes;

namespace Sitecore.Rocks.UI.Commandy.Searchers
{
    [Export(typeof(ISearcher))]
    public class SearchSearcher : SearcherBase
    {
        private string text;

        public override void Search(CommandyContext context)
        {
            Assert.ArgumentNotNull(context, nameof(context));

            var mode = context.Mode as SearchBasedMode;
            if (mode == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(context.Text) || context.Text == text)
            {
                return;
            }

            mode.IsReady = false;
            text = context.Text;

            var s = context.Commandy.Parameter as ISiteSelectionContext;
            if (s == null)
            {
                return;
            }

            GetItemsCompleted<ItemHeader> completed = delegate(IEnumerable<ItemHeader> items)
            {
                var hits = new List<Hit>();

                foreach (var itemHeader in items)
                {
                    var rank = 2;
                    if (itemHeader.Name.StartsWith(context.Text, StringComparison.InvariantCultureIgnoreCase))
                    {
                        rank = 0;
                    }
                    else if (itemHeader.Name.IsFilterMatch(context.Text))
                    {
                        rank = 1;
                    }

                    var hit = new Hit(string.Format(@"{0} - [{1}, {2}]", itemHeader.Name, itemHeader.TemplateName, itemHeader.Path), itemHeader, rank);

                    hits.Add(hit);
                }

                context.Commandy.AddHits(hits);

                mode.IsReady = true;
            };

            s.Site.DataService.Search(context.Text, new DatabaseUri(s.Site, DatabaseName.Master), string.Empty, string.Empty, ItemUri.Empty, 0, completed);
        }

        public override void SetActiveMode(IMode mode)
        {
            Assert.ArgumentNotNull(mode, nameof(mode));

            base.SetActiveMode(mode);

            text = null;
        }
    }
}
