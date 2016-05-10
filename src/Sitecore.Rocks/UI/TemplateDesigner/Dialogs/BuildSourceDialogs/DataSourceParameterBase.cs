// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.TemplateDesigner.Dialogs.BuildSourceDialogs
{
    public abstract class DataSourceParameterBase : IDataSourceParameter
    {
        public virtual bool CanExecute(DatabaseUri databaseUri, [NotNull] BuildSourceField field)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(field, nameof(field));

            return true;
        }

        public abstract DataSourceString Execute(DatabaseUri databaseUri, BuildSourceField field, DataSourceString dataSource);

        protected void SetDataSource([NotNull] DataSourceString dataSource)
        {
            Debug.ArgumentNotNull(dataSource, nameof(dataSource));

            var path = dataSource.Path;
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (path.StartsWith(@"query:", StringComparison.InvariantCultureIgnoreCase))
            {
                dataSource.Path = string.Empty;
                return;
            }

            dataSource["datasource"] = path;
            dataSource.Path = string.Empty;
        }
    }
}
