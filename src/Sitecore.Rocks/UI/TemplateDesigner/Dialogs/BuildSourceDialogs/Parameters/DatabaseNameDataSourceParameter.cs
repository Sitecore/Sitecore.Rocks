// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.UI.Dialogs.SelectDatabaseDialogs;

namespace Sitecore.Rocks.UI.TemplateDesigner.Dialogs.BuildSourceDialogs.Parameters
{
    [DataSourceParameter("Database (databasename)", 1000)]
    public class DatabaseNameDataSourceParameter : DataSourceParameterBase
    {
        public override DataSourceString Execute(DatabaseUri databaseUri, BuildSourceField field, DataSourceString dataSource)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(field, nameof(field));
            Assert.ArgumentNotNull(dataSource, nameof(dataSource));

            var dialog = new SelectDatabaseDialog
            {
                SelectedDatabaseUri = databaseUri
            };

            if (!dialog.ShowDialog())
            {
                return null;
            }

            var d = dialog.SelectedDatabaseUri;
            if (d == DatabaseUri.Empty)
            {
                return null;
            }

            SetDataSource(dataSource);
            dataSource["databasename"] = d.DatabaseName.ToString();

            return dataSource;
        }
    }
}
