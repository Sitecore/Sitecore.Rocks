// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System.ComponentModel;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Commands.Searching
{
    public abstract class SearchCommand : CommandBase
    {
        protected SearchCommand()
        {
            Group = "Search";
        }

        [Localizable(false)]
        protected string FieldName { get; set; }

        [Localizable(false)]
        protected string FieldPath { get; set; }

        protected string Value { get; set; }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return false;
            }

            var selectedItems = context.Items.ToList();
            if (selectedItems.Count() != 1)
            {
                return false;
            }

            var item = selectedItems.First();
            if (item == null)
            {
                return false;
            }

            return (item.ItemUri.Site.DataService.FeatureCapabilities & DataServiceFeatureCapabilities.Execute) == DataServiceFeatureCapabilities.Execute;
        }

        public override void Execute(object parameter)
        {
            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return;
            }

            var selectedItems = context.Items.ToList();
            if (selectedItems.Count() != 1)
            {
                return;
            }

            var item = selectedItems.First();
            if (item == null)
            {
                return;
            }

            var searchViewer = AppHost.Windows.OpenSearchViewer(item.ItemUri.Site);
            if (searchViewer == null)
            {
                return;
            }

            var value = GetValue(item);

            searchViewer.Search(FieldName, item.ItemUri, value);
        }

        [NotNull]
        protected virtual string GetValue([NotNull] IItem item)
        {
            Debug.ArgumentNotNull(item, nameof(item));

            return Value;
        }
    }
}
