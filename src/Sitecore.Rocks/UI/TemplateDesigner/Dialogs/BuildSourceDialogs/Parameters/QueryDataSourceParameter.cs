// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Controls.QueryBuilders;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Extensions.StringExtensions;

namespace Sitecore.Rocks.UI.TemplateDesigner.Dialogs.BuildSourceDialogs.Parameters
{
    [DataSourceParameter("Query (query)", 200)]
    public class QueryDataSourceParameter : DataSourceParameterBase
    {
        public override bool CanExecute(DatabaseUri databaseUri, BuildSourceField field)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(field, nameof(field));

            if (string.Compare(field.Type, @"tree list", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return false;
            }

            if (string.Compare(field.Type, @"treelist", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return false;
            }

            if (string.Compare(field.Type, @"treelistex", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return false;
            }

            if (string.Compare(field.Type, @"droptree", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return false;
            }

            if (string.Compare(field.Type, @"reference", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return false;
            }

            return true;
        }

        public override DataSourceString Execute(DatabaseUri databaseUri, BuildSourceField field, DataSourceString dataSource)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(field, nameof(field));
            Assert.ArgumentNotNull(dataSource, nameof(dataSource));

            var query = dataSource.Path;

            if (query.StartsWith(@"query:", StringComparison.InvariantCultureIgnoreCase))
            {
                query = query.Mid(6);
            }

            var dialog = new BuildQueryDialog(query, CustomValidationType.Query);
            dialog.Title = "Build Query";
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return null;
            }

            dataSource.Path = @"query:" + dialog.Text;
            dataSource.Parameters.Clear();

            return dataSource;
        }
    }
}
