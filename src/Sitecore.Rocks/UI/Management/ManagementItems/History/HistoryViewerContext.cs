// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Management.ManagementItems.History
{
    public class HistoryViewerContext : ICommandContext, IItemSelectionContext, IDatabaseSelectionContext, ISiteSelectionContext
    {
        public HistoryViewerContext([NotNull] HistoryViewer historyViewer)
        {
            Assert.ArgumentNotNull(historyViewer, nameof(historyViewer));

            HistoryViewer = historyViewer;
        }

        public HistoryViewer HistoryViewer { get; }

        DatabaseUri IDatabaseSelectionContext.DatabaseUri
        {
            get { return HistoryViewer.Context.DatabaseUri; }
        }

        IEnumerable<IItem> IItemSelectionContext.Items
        {
            get
            {
                foreach (var selectedItem in HistoryViewer.History.SelectedItems)
                {
                    var entry = selectedItem as IItem;
                    if (entry != null)
                    {
                        yield return entry;
                    }
                }
            }
        }

        Site ISiteSelectionContext.Site
        {
            get { return HistoryViewer.Context.DatabaseUri.Site; }
        }
    }
}
