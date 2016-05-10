// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.WindowExtensions;
using Sitecore.Rocks.UI.LayoutDesigners.Items;

namespace Sitecore.Rocks.UI.LayoutDesigners.Dialogs.SelectColumnsDialogs
{
    public partial class SelectColumnsDialog
    {
        public SelectColumnsDialog(IEnumerable<RenderingItem> renderings, IEnumerable<string> selectedColumns)
        {
            SelectColumns = new List<string>();

            Renderings = renderings;
            SelectColumns.AddRange(selectedColumns);

            InitializeComponent();
            this.InitializeDialog();

            Loaded += ControlLoaded;
        }

        public IEnumerable<RenderingItem> Renderings { get; }

        public List<string> SelectColumns { get; }

        internal void ControlLoaded([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            Loaded -= ControlLoaded;

            RenderRenderings(Renderings);
        }

        private IEnumerable<object> GetProperties(RenderingItem renderingItem)
        {
            var properties = TypeDescriptor.GetProperties(renderingItem);

            yield return "Rendering";

            foreach (PropertyDescriptor property in properties)
            {
                if (property.Attributes.OfType<BrowsableAttribute>().Any(a => !a.Browsable))
                {
                    continue;
                }

                var columnName = property.Name;

                switch (columnName)
                {
                    case "Id":
                        columnName = "ID";
                        break;
                    case "DataSource":
                        columnName = "Data Source";
                        break;
                    case "PlaceholderKey":
                        columnName = "Placeholder";
                        break;
                }

                yield return columnName;
            }
        }

        private void OkClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Debug.ArgumentNotNull(sender, nameof(sender));
            Debug.ArgumentNotNull(e, nameof(e));

            var newSelectedColumns = ColumnsStackPanel.Children.OfType<CheckBox>().Where(c => c.IsChecked == true).Select(c => c.Tag as string);

            SelectColumns.RemoveAll(c => !newSelectedColumns.Contains(c));
            SelectColumns.AddRange(newSelectedColumns.Where(c => !SelectColumns.Contains(c)));

            this.Close(true);
        }

        private void RenderRenderings(IEnumerable<RenderingItem> renderings)
        {
            var columns = renderings.SelectMany(GetProperties).Distinct();

            foreach (var column in columns.OrderBy(c => c))
            {
                var columnName = column as string;

                var checkBox = new CheckBox
                {
                    Tag = columnName,
                    Content = columnName,
                    IsChecked = SelectColumns.Contains(columnName),
                    IsEnabled = columnName != "Rendering"
                };

                ColumnsStackPanel.Children.Add(checkBox);
            }

            OkButton.IsEnabled = true;
        }
    }
}
