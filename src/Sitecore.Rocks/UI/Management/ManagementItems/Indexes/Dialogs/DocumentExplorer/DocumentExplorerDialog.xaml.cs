// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Windows;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Indexes.Dialogs.DocumentExplorer
{
    public partial class DocumentExplorerDialog : IContextProvider
    {
        public DocumentExplorerDialog([NotNull] Site site, [NotNull] string indexName, [NotNull] string fieldName, [NotNull] string term)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(indexName, nameof(indexName));
            Assert.ArgumentNotNull(fieldName, nameof(fieldName));
            Assert.ArgumentNotNull(term, nameof(term));

            InitializeComponent();
            this.InitializeDialog();

            Site = site;
            IndexName = indexName;
            FieldName = fieldName;
            Term = term;

            DocumentList.Site = site;
            IndexNameLabel.Content = "Documents in the \"" + Term + "\" term:";

            Loaded += ControlLoaded;
        }

        public string FieldName { get; set; }

        public string IndexName { get; set; }

        [NotNull]
        public Site Site { get; set; }

        public string Term { get; set; }

        [NotNull]
        public object GetContext()
        {
            return new DocumentExplorerContext(this);
        }

        public void LoadDocuments(int offset)
        {
            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                Loading.HideLoading(DocumentList);

                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                DocumentList.Load(root, offset);
            };

            Loading.ShowLoading(DocumentList);

            Site.DataService.ExecuteAsync("Indexes.GetDocuments", callback, IndexName, FieldName, Term, offset.ToString());
        }

        public void Refresh()
        {
            LoadDocuments(0);
        }

        private void CloseClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            this.Close(false);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            LoadDocuments(0);
        }

        private void PageChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            LoadDocuments(DocumentList.Offset);
        }
    }
}
