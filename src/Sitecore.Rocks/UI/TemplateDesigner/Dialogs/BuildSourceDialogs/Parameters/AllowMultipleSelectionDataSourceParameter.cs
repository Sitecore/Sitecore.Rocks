// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.TemplateDesigner.Dialogs.BuildSourceDialogs.Parameters
{
    [DataSourceParameter("Allow Multiple Selection (allowmultipleselection)", 1000)]
    public class AllowMultipleSelectionDataSourceParameter : DataSourceParameterBase
    {
        public override bool CanExecute(DatabaseUri databaseUri, BuildSourceField field)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(field, nameof(field));

            return string.Compare(field.Type, @"tree list", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(field.Type, @"treelist", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(field.Type, @"treelistex", StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        [NotNull]
        public override DataSourceString Execute(DatabaseUri databaseUri, BuildSourceField field, DataSourceString dataSource)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(field, nameof(field));
            Assert.ArgumentNotNull(dataSource, nameof(dataSource));

            dataSource["allowmultipleselection"] = @"yes";

            return dataSource;
        }
    }
}
