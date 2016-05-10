// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.TemplateDesigner.Dialogs.BuildSourceDialogs.Parameters
{
    public abstract class PromptDataSourceParameterBase : DataSourceParameterBase
    {
        public override DataSourceString Execute(DatabaseUri databaseUri, BuildSourceField field, DataSourceString dataSource)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(field, nameof(field));
            Assert.ArgumentNotNull(dataSource, nameof(dataSource));

            var key = GetKey();

            var value = dataSource[key] ?? string.Empty;

            var result = AppHost.Prompt(GetText(), "Data Source Parameter", value);
            if (result == null)
            {
                return null;
            }

            SetDataSource(dataSource);
            dataSource[key] = result;

            return dataSource;
        }

        [NotNull]
        protected abstract string GetKey();

        [NotNull]
        protected abstract string GetText();
    }
}
