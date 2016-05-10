// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;
using Sitecore.Rocks.Sites;
using Sitecore.Rocks.UI.TemplateDesigner.Dialogs.BuildSourceDialogs;
using Sitecore.Rocks.UI.TemplateDesigners.Dialogs.BuildSourceDialogs.Parameters.BindModeDialogs;

namespace Sitecore.Rocks.UI.TemplateDesigners.Dialogs.BuildSourceDialogs.Parameters
{
    [DataSourceParameter("Bind Mode (SPEAK)", 1100)]
    public class BindModeParameter : DataSourceParameterBase
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

            var bindMode = dataSource["bindmode"] ?? string.Empty;

            var dialog = new BindModeDialog(bindMode);
            if (AppHost.Shell.ShowDialog(dialog) != true)
            {
                return null;
            }

            bindMode = dialog.BindMode;

            SetDataSource(dataSource);
            if (string.IsNullOrEmpty(bindMode))
            {
                dataSource.Remove("bindmode");
            }
            else
            {
                dataSource["bindmode"] = bindMode;
            }

            return dataSource;
        }
    }
}
