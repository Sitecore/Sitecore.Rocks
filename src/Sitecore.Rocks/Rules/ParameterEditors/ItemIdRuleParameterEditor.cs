// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.Linq;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;

namespace Sitecore.Rocks.Rules.ParameterEditors
{
    [RuleParameter("ItemId")]
    public class ItemIdRuleParameterEditor : IRuleParameterEditor
    {
        [NotNull]
        public string GetValue(string defaultValue, object parameter)
        {
            Assert.ArgumentNotNull(defaultValue, nameof(defaultValue));

            var context = parameter as IItemSelectionContext;
            if (context == null)
            {
                return GetPath(defaultValue);
            }

            if (!context.Items.Any())
            {
                return GetPath(defaultValue);
            }

            return GetItemId(context, defaultValue);
        }

        [NotNull]
        private string GetItemId([NotNull] IItemSelectionContext context, [NotNull] string defaultValue)
        {
            Debug.ArgumentNotNull(context, nameof(context));
            Debug.ArgumentNotNull(defaultValue, nameof(defaultValue));

            var itemId = ItemId.Empty;
            if (!string.IsNullOrEmpty(defaultValue))
            {
                Guid guid;
                if (Guid.TryParse(defaultValue, out guid))
                {
                    itemId = new ItemId(guid);
                }
            }

            var databaseUri = context.Items.First().ItemUri.DatabaseUri;
            var itemUri = new ItemUri(databaseUri, itemId);

            var dialog = new SelectItemDialog();

            dialog.Initialize(Resources.Browse, itemUri);

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return defaultValue;
            }

            return dialog.SelectedItemUri.ItemId.ToString();
        }

        [NotNull]
        private string GetPath([NotNull] string defaultValue)
        {
            Debug.ArgumentNotNull(defaultValue, nameof(defaultValue));

            var result = AppHost.Prompt(defaultValue, @"Enter an item path:");

            return result ?? defaultValue;
        }
    }
}
