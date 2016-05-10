// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Controls;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Data.DataServices;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.ListViewExtensions;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.Management.ManagementItems.Indexes.Dialogs.TermExplorer.Commands;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Indexes.Dialogs.TermExplorer
{
    public partial class TermExplorerDialog : IContextProvider
    {
        private readonly ListViewSorter listViewSorter;

        private readonly List<TermDescriptor> terms = new List<TermDescriptor>();

        public TermExplorerDialog([NotNull] Site site, [NotNull] string indexName, [NotNull] string fieldName)
        {
            Assert.ArgumentNotNull(site, nameof(site));
            Assert.ArgumentNotNull(indexName, nameof(indexName));
            Assert.ArgumentNotNull(fieldName, nameof(fieldName));

            InitializeComponent();
            this.InitializeDialog();

            Site = site;
            IndexName = indexName;
            FieldName = fieldName;
            FilterText = string.Empty;

            IndexNameLabel.Content = "Terms in the \"" + FieldName + "\" field:";
            listViewSorter = new ListViewSorter(TermList);

            Loaded += ControlLoaded;
        }

        public string FieldName { get; set; }

        public string IndexName { get; set; }

        [NotNull]
        public Site Site { get; set; }

        protected string FilterText { get; set; }

        [NotNull]
        public object GetContext()
        {
            return new TermExplorerContext(this);
        }

        public void LoadTerms()
        {
            ExecuteCompleted callback = delegate(string response, ExecuteResult executeResult)
            {
                Loading.HideLoading(ContextMenuPanel);

                if (!DataService.HandleExecute(response, executeResult))
                {
                    return;
                }

                var root = response.ToXElement();
                if (root == null)
                {
                    return;
                }

                ParseTerms(root);
                RenderTerms();
            };

            Loading.ShowLoading(ContextMenuPanel);

            Site.DataService.ExecuteAsync("Indexes.GetTerms", callback, IndexName, FieldName);
        }

        public void Refresh()
        {
            LoadTerms();
        }

        private void CloseClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            this.Close(false);
        }

        private void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            LoadTerms();
        }

        private void ExploreDocuments([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = GetContext();

            var command = new ExploreDocuments();
            if (command.CanExecute(context))
            {
                AppHost.Usage.ReportCommand(command, context);
                command.Execute(context);
            }
        }

        private void FilterChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            FilterText = Filter.Text;
            RenderTerms();
        }

        private void HeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            listViewSorter.HeaderClick(sender, e);
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = GetContext();

            ContextMenuPanel.ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void ParseTerms([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            var list = new List<TermDescriptor>();

            foreach (var element in root.Elements())
            {
                var termDescriptor = new TermDescriptor
                {
                    Text = element.Value,
                    DocumentCount = element.GetAttributeInt("documents", 0)
                };

                list.Add(termDescriptor);
            }

            terms.Clear();
            terms.AddRange(list);
        }

        private void RenderTerms()
        {
            var list = new List<TermDescriptor>();
            foreach (var termDescriptor in terms)
            {
                if (termDescriptor.Text.IsFilterMatch(FilterText))
                {
                    list.Add(termDescriptor);
                }
            }

            TermList.ItemsSource = null;
            TermList.ItemsSource = list.OrderBy(c => c.Text);
            listViewSorter.Resort();

            TermList.ResizeColumn(TermColumn);
            TermList.ResizeColumn(DocumentCountColumn);

            if (terms.Count > 0)
            {
                TermList.SelectedIndex = 0;
            }
        }

        public class TermDescriptor
        {
            public int DocumentCount { get; set; }

            [NotNull]
            public string FormattedDocumentCount
            {
                get { return DocumentCount.ToString("#,##0"); }
            }

            public string Text { get; set; }
        }
    }
}
