// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.TemplateDesigner.Dialogs.BuildSourceDialogs.Parameters
{
    [DataSourceParameter("Exclude Templates For Selection (excludetemplatesforselection)", 1000)]
    public class ExcludeTemplatesForSelection : ItemsDataSourceParameterBase
    {
        public override bool CanExecute(DatabaseUri databaseUri, BuildSourceField field)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(field, nameof(field));

            return string.Compare(field.Type, @"tree list", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(field.Type, @"treelist", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(field.Type, @"treelistex", StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        protected override string GetKey()
        {
            return @"excludetemplatesforselection";
        }
    }
}
