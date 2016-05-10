// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Data;

namespace Sitecore.Rocks.UI.TemplateDesigner.Dialogs.BuildSourceDialogs
{
    public interface IDataSourceParameter
    {
        bool CanExecute([NotNull] DatabaseUri databaseUri, BuildSourceField field);

        [CanBeNull]
        DataSourceString Execute([NotNull] DatabaseUri databaseUri, [NotNull] BuildSourceField field, [NotNull] DataSourceString dataSource);
    }
}
