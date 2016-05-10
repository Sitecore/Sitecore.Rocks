// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.QueryAnalyzers
{
    public class QueryAnalyzerContext : ICommandContext, IItemSelectionContext, IDatabaseUriContext, ISiteSelectionContext, IItemsContext
    {
        public QueryAnalyzerContext([NotNull] QueryAnalyzer queryAnalyzer, [NotNull] string batchScript, [NotNull] List<IItem> selectedItems)
        {
            Assert.ArgumentNotNull(queryAnalyzer, nameof(queryAnalyzer));
            Assert.ArgumentNotNull(batchScript, nameof(batchScript));
            Assert.ArgumentNotNull(selectedItems, nameof(selectedItems));

            QueryAnalyzer = queryAnalyzer;
            Script = batchScript;
            SelectedItems = selectedItems;
        }

        [NotNull]
        public QueryAnalyzer QueryAnalyzer { get; }

        [NotNull]
        public string Script { get; set; }

        [NotNull]
        public IEnumerable<IItem> SelectedItems { get; }

        DatabaseUri IDatabaseSelectionContext.DatabaseUri
        {
            get { return QueryAnalyzer.DatabaseUri; }
        }

        IEnumerable<IItem> IItemsContext.Items
        {
            get { return QueryAnalyzer.DataTables.SelectMany(dataTable => dataTable.Rows.Cast<QueryAnalyzer.ResultDataRow>()); }
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

            QueryAnalyzer.SetDatabaseUri(databaseUri);
        }
    }
}
