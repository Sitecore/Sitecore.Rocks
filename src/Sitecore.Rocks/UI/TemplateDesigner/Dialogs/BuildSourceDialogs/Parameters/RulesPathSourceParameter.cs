// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;

namespace Sitecore.Rocks.UI.TemplateDesigner.Dialogs.BuildSourceDialogs.Parameters
{
    [DataSourceParameter("Rules Path (rulespath)", 100)]
    public class RulesPathSourceParameter : DataSourceParameterBase
    {
        public override bool CanExecute(DatabaseUri databaseUri, BuildSourceField field)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(field, nameof(field));

            return string.Compare(field.Type, @"rules", StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public override DataSourceString Execute(DatabaseUri databaseUri, BuildSourceField field, DataSourceString dataSource)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(field, nameof(field));
            Assert.ArgumentNotNull(dataSource, nameof(dataSource));

            var dialog = new SelectItemDialog();

            var itemId = new ItemId(DatabaseTreeViewItem.RootItemGuid);

            var value = dataSource["rulespath"];
            if (string.IsNullOrEmpty(value))
            {
                var itemUri = new ItemUri(databaseUri, itemId);
                dialog.Initialize(Resources.Browse, itemUri);
            }
            else
            {
                Guid guid;
                if (Guid.TryParse(value, out guid))
                {
                    itemId = new ItemId(guid);
                    var itemUri = new ItemUri(databaseUri, itemId);
                    dialog.Initialize(Resources.Browse, itemUri);
                }
                else
                {
                    dialog.Initialize(Resources.Browse, databaseUri, value);
                }
            }

            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return null;
            }

            dataSource["rulespath"] = dialog.SelectedItemUri.ItemId.ToString();

            return dataSource;
        }
    }
}
