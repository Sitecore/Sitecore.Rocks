// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Publishing
{
    public class PublishingQueueContext : ICommandContext, IDatabaseUriContext, IItemSelectionContext, ISiteSelectionContext
    {
        public PublishingQueueContext([NotNull] PublishingQueue publishingQueueViewer)
        {
            Assert.ArgumentNotNull(publishingQueueViewer, nameof(publishingQueueViewer));

            PublishingQueueViewer = publishingQueueViewer;
        }

        [NotNull]
        public PublishingQueue PublishingQueueViewer { get; }

        [NotNull]
        public IEnumerable<ItemHeader> SelectedItems { get; set; }

        DatabaseUri IDatabaseSelectionContext.DatabaseUri
        {
            get { return PublishingQueueViewer.DatabaseUri; }
        }

        IEnumerable<IItem> IItemSelectionContext.Items
        {
            get { return SelectedItems; }
        }

        Site ISiteSelectionContext.Site
        {
            get { return ((IDatabaseSelectionContext)this).DatabaseUri.Site; }
        }

        void IDatabaseUriContext.SetDatabaseUri(DatabaseUri databaseUri)
        {
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            PublishingQueueViewer.SetDatabaseUri(databaseUri);
        }
    }
}
