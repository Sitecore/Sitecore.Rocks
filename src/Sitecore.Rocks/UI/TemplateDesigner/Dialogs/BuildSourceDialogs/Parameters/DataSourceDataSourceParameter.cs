// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;

namespace Sitecore.Rocks.UI.TemplateDesigner.Dialogs.BuildSourceDialogs.Parameters
{
    [DataSourceParameter("Data Source (datasource)", 100)]
    public class DataSourceDataSourceParameter : DataSourceParameterBase
    {
        public override DataSourceString Execute(DatabaseUri databaseUri, BuildSourceField field, DataSourceString dataSource)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(field, nameof(field));
            Assert.ArgumentNotNull(dataSource, nameof(dataSource));

            var itemId = new ItemId(DatabaseTreeViewItem.RootItemGuid);

            var value = dataSource["datasource"];
            if (!string.IsNullOrEmpty(value))
            {
                Guid guid;
                if (Guid.TryParse(value, out guid))
                {
                    itemId = new ItemId(guid);
                }
            }

            var itemUri = new ItemUri(databaseUri, itemId);

            var dialog = new SelectItemDialog();

            dialog.Initialize(Resources.Browse, itemUri);
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return null;
            }

            dataSource.Path = string.Empty;
            dataSource["datasource"] = dialog.SelectedItemUri.ItemId.ToString();

            return dataSource;
        }
    }
}
