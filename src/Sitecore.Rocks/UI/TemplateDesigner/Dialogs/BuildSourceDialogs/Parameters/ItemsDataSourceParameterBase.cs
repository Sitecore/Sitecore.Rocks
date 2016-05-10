// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.ContentTrees.Items;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;
using Sitecore.Rocks.UI.Dialogs.SelectItemDialogs;

namespace Sitecore.Rocks.UI.TemplateDesigner.Dialogs.BuildSourceDialogs.Parameters
{
    public abstract class ItemsDataSourceParameterBase : DataSourceParameterBase
    {
        public override DataSourceString Execute(DatabaseUri databaseUri, BuildSourceField field, DataSourceString dataSource)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(field, nameof(field));
            Assert.ArgumentNotNull(dataSource, nameof(dataSource));

            var itemId = new ItemId(DatabaseTreeViewItem.RootItemGuid);

            var key = GetKey();

            var value = dataSource[key] ?? string.Empty;
            var n = value.IndexOf(@"|", StringComparison.Ordinal);
            if (n >= 0)
            {
                value = value.Left(n);
            }

            if (!string.IsNullOrEmpty(value))
            {
                Guid guid;
                if (Guid.TryParse(value, out guid))
                {
                    itemId = new ItemId(guid);
                }
            }

            var itemUri = new ItemUri(databaseUri, itemId);

            var dialog = new SelectItemDialog
            {
                AllowMultipleSelection = true
            };

            dialog.Initialize(Resources.Browse, itemUri);
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return null;
            }

            value = string.Empty;

            foreach (var name in dialog.SelectedItemNames)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    value += @"|";
                }

                value += name;
            }

            SetDataSource(dataSource);
            dataSource[key] = value;

            return dataSource;
        }

        [NotNull]
        protected abstract string GetKey();
    }
}
