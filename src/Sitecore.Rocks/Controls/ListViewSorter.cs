// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.Controls
{
    public class ListViewSorter
    {
        private readonly ListView _listView;

        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        private string _lastHeader;

        private GridViewColumnHeader _lastHeaderClicked;

        public ListViewSorter([NotNull] ListView listView)
        {
            Assert.ArgumentNotNull(listView, nameof(listView));

            _listView = listView;
        }

        public void HeaderClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(e, nameof(e));

            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked == null)
            {
                return;
            }

            if (headerClicked.Role == GridViewColumnHeaderRole.Padding)
            {
                return;
            }

            ListSortDirection direction;
            if (!headerClicked.Equals(_lastHeaderClicked))
            {
                direction = ListSortDirection.Ascending;
            }
            else
            {
                if (_lastDirection == ListSortDirection.Ascending)
                {
                    direction = ListSortDirection.Descending;
                }
                else
                {
                    direction = ListSortDirection.Ascending;
                }
            }

            var header = headerClicked.Column.Header as string;

            var binding = headerClicked.Column.DisplayMemberBinding as Binding;
            if (binding != null)
            {
                var path = binding.Path;
                if (path != null)
                {
                    var propertyPath = path.Path;
                    if (!string.IsNullOrEmpty(propertyPath))
                    {
                        header = propertyPath;
                    }
                }
            }

            Sort(header, direction);

            if (direction == ListSortDirection.Ascending)
            {
                headerClicked.Column.HeaderTemplate = _listView.Resources[@"HeaderTemplateArrowUp"] as DataTemplate;
            }
            else
            {
                headerClicked.Column.HeaderTemplate = _listView.Resources[@"HeaderTemplateArrowDown"] as DataTemplate;
            }

            if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
            {
                _lastHeaderClicked.Column.HeaderTemplate = null;
            }

            _lastHeader = header;
            _lastHeaderClicked = headerClicked;
            _lastDirection = direction;
        }

        public void Resort()
        {
            if (_lastHeader != null)
            {
                Sort(_lastHeader, _lastDirection);
            }
        }

        private void Sort([CanBeNull] string sortBy, ListSortDirection direction)
        {
            if (sortBy == null)
            {
                return;
            }

            var dataView = CollectionViewSource.GetDefaultView(_listView.ItemsSource);
            if (dataView == null)
            {
                return;
            }

            dataView.SortDescriptions.Clear();
            var sd = new SortDescription(sortBy, direction);

            try
            {
                dataView.SortDescriptions.Add(sd);
            }
            catch (Exception ex)
            {
                AppHost.Output.LogException(ex);
            }

            dataView.Refresh();
        }
    }
}
