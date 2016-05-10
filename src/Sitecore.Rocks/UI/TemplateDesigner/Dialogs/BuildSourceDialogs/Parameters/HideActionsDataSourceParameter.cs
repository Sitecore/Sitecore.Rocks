// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.TemplateDesigner.Dialogs.BuildSourceDialogs.Parameters
{
    [DataSourceParameter("Hide Actions (hideactions)", 1000)]
    public class HideActionsDataSourceParameter : DataSourceParameterBase
    {
        public override bool CanExecute(DatabaseUri databaseUri, BuildSourceField field)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(field, nameof(field));

            return string.Compare(field.Type, @"rules", StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        [NotNull]
        public override DataSourceString Execute(DatabaseUri databaseUri, BuildSourceField field, DataSourceString dataSource)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(field, nameof(field));
            Assert.ArgumentNotNull(dataSource, nameof(dataSource));

            dataSource["hideactions"] = @"true";

            return dataSource;
        }
    }
}
