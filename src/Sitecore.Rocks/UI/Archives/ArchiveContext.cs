// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Archives
{
    public class ArchiveContext : ICommandContext, ISiteSelectionContext, IDatabaseUriContext
    {
        public ArchiveContext([NotNull] ArchiveViewer archiveViewer)
        {
            Assert.ArgumentNotNull(archiveViewer, nameof(archiveViewer));

            ArchiveViewer = archiveViewer;
        }

        [NotNull]
        public ArchiveViewer ArchiveViewer { get; }

        [NotNull]
        public IEnumerable<ArchiveEntry> SelectedItems { get; set; }

        DatabaseUri IDatabaseSelectionContext.DatabaseUri
        {
            get { return ArchiveViewer.DatabaseUri; }
        }

        Site ISiteSelectionContext.Site
        {
            get { return ((IDatabaseSelectionContext)this).DatabaseUri.Site; }
        }

        void IDatabaseUriContext.SetDatabaseUri(DatabaseUri databaseUri)
        {
            Debug.ArgumentNotNull(databaseUri, nameof(databaseUri));

            ArchiveViewer.SetDatabaseUri(databaseUri);
        }
    }
}
