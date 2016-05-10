// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.XElementExtensions;
using Sitecore.Rocks.Sites;

namespace Sitecore.Rocks.UI.Management.ManagementItems.Indexes.Dialogs
{
    public partial class DocumentList : IContextProvider
    {
        public delegate void PageChangedEventHandler(object sender, EventArgs e);

        private readonly List<string> documentColumns = new List<string>();

        private readonly List<DocumentDescriptor> documents = new List<DocumentDescriptor>();

        private int documentCount;

        private int documentTotalCount;

        public DocumentList()
        {
            Offset = 0;
            InitializeComponent();
        }

        public int Offset { get; set; }

        public Site Site { get; set; }

        public void Clear()
        {
            documentCount = 0;
            documentTotalCount = 0;
            documents.Clear();
            DataGrid.ItemsSource = null;
        }

        [NotNull]
        public object GetContext()
        {
            return new DocumentListContext(this);
        }

        public void Load([NotNull] XElement root, int offset)
        {
            Assert.ArgumentNotNull(root, nameof(root));

            Offset = offset;

            ParseDocuments(root);
            ParseColumns(root);

            RenderHits();
            RenderPager();
        }

        public event PageChangedEventHandler PageChanged;

        private void NextDocument([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (Offset >= documentTotalCount - 100)
            {
                return;
            }

            if (Offset >= documentTotalCount - 100)
            {
                return;
            }

            Offset += 100;
            RaisePageChanged();
        }

        private void OpenContextMenu([NotNull] object sender, [NotNull] ContextMenuEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var context = GetContext();

            DataGridBorder.ContextMenu = AppHost.ContextMenus.Build(context, e);
        }

        private void ParseColumns([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            documentColumns.Clear();

            var columns = root.Element("columns");
            if (columns == null)
            {
                return;
            }

            foreach (var element in columns.Elements())
            {
                var name = element.GetAttributeValue("name");

                documentColumns.Add(name);
            }
        }

        private void ParseDocuments([NotNull] XElement root)
        {
            Debug.ArgumentNotNull(root, nameof(root));

            documents.Clear();

            var documentElement = root.Element("documents");
            if (documentElement == null)
            {
                return;
            }

            documentCount = documentElement.GetAttributeInt("count", 0);
            documentTotalCount = documentElement.GetAttributeInt("total", 0);

            foreach (var element in documentElement.Elements())
            {
                var hitDescriptor = new DocumentDescriptor();
                documents.Add(hitDescriptor);

                foreach (var f in element.Elements())
                {
                    var field = new DocumentFieldDescriptor
                    {
                        Name = f.GetAttributeValue("name"),
                        Value = f.Value
                    };

                    hitDescriptor.Fields.Add(field);
                }
            }
        }

        private void PreviousDocument([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            if (Offset == 0)
            {
                return;
            }

            if (Offset > 100)
            {
                Offset -= 100;
            }
            else
            {
                Offset = 0;
            }

            RaisePageChanged();
        }

        private void RaisePageChanged()
        {
            var pageChanged = PageChanged;
            if (pageChanged != null)
            {
                pageChanged(this, EventArgs.Empty);
            }
        }

        private void RenderHits()
        {
            var dataTable = new DataTable();

            foreach (var hitColumn in documentColumns)
            {
                dataTable.Columns.Add(hitColumn, typeof(string));
            }

            foreach (var hitDescriptor in documents)
            {
                var dataRow = dataTable.NewRow();

                foreach (var field in hitDescriptor.Fields)
                {
                    dataRow[field.Name] = field.Value;
                }

                dataTable.Rows.Add(dataRow);
            }

            DataGrid.ItemsSource = dataTable.DefaultView;
        }

        private void RenderPager()
        {
            if (documentCount == 0)
            {
                Pager.Text = "No hits found.";
            }
            else
            {
                Pager.Text = string.Format("Showing {0} to {1} of {2}", Offset + 1, Offset + documents.Count, documentTotalCount);
            }

            PreviousButton.IsEnabled = Offset > 0;
            NextButton.IsEnabled = Offset < documentTotalCount - 100;
        }

        public class DocumentDescriptor
        {
            public DocumentDescriptor()
            {
                Fields = new List<DocumentFieldDescriptor>();
            }

            public List<DocumentFieldDescriptor> Fields { get; set; }
        }

        public class DocumentFieldDescriptor
        {
            public string Name { get; set; }

            public string Value { get; set; }
        }
    }
}
