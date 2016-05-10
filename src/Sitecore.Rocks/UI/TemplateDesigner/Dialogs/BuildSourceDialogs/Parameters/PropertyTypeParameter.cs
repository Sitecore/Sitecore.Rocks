// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.TemplateDesigner.Dialogs.BuildSourceDialogs;
using Sitecore.Rocks.UI.TemplateDesigners.Dialogs.BuildSourceDialogs.Parameters.PropertyTypeDialogs;

namespace Sitecore.Rocks.UI.TemplateDesigners.Dialogs.BuildSourceDialogs.Parameters
{
    [DataSourceParameter("Property Type (SPEAK)", 1100)]
    public class PropertyTypeParameter : DataSourceParameterBase
    {
        public override bool CanExecute(DatabaseUri databaseUri, BuildSourceField field)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(field, nameof(field));

            return databaseUri.Site.IsSitecoreVersion(SitecoreVersion.Version70);
        }

        public override DataSourceString Execute(DatabaseUri databaseUri, BuildSourceField field, DataSourceString dataSource)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(field, nameof(field));
            Assert.ArgumentNotNull(dataSource, nameof(dataSource));

            var propertyType = dataSource["propertytype"] ?? string.Empty;

            var dialog = new PropertyTypeDialog(propertyType);
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return null;
            }

            propertyType = dialog.PropertyType;

            SetDataSource(dataSource);
            if (string.IsNullOrEmpty(propertyType))
            {
                dataSource.Remove("propertytype");
            }
            else
            {
                dataSource["propertytype"] = propertyType;
            }

            return dataSource;
        }
    }
}
