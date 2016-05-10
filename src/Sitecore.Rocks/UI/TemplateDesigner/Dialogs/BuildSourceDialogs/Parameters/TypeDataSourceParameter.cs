// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.UI.TemplateDesigner.Dialogs.BuildSourceDialogs.Parameters
{
    [DataSourceParameter("Type and Assembly (type)", 1000)]
    public class TypeDataSourceParameter : PromptDataSourceParameterBase
    {
        public override bool CanExecute(DatabaseUri databaseUri, BuildSourceField field)
        {
            Assert.ArgumentNotNull(databaseUri, nameof(databaseUri));
            Assert.ArgumentNotNull(field, nameof(field));

            return string.Compare(field.Type, @"custom", StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        protected override string GetKey()
        {
            return @"type";
        }

        protected override string GetText()
        {
            return @"Enter type and assembly, e.g. MyNamespace.MyType,MyAssembly.dll:";
        }
    }
}
